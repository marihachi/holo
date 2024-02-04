import { FunctionSymbol, UnitSymbol, VariableSymbol } from './semantic-node.js';
import { SyntaxNode } from './syntax-node.js';

export function emit(unitSymbol: UnitSymbol): string {
  let code = '';

  // setup ISA
  code += 'target triple = "x86_64-unknown-linux-gnu"\n';

  for (const decl of unitSymbol.node.decls) {
    switch (decl.kind) {
      case 'FunctionDeclNode': {
        const funcSymbol = unitSymbol.nameTable.get(decl.name)! as FunctionSymbol;
        const f = new FunctionContext();
        emitInstruction(f, funcSymbol.node, unitSymbol, funcSymbol, undefined);
        // emit code
        const args = funcSymbol.node.parameters
          .map(x => `i32 %${ x.name }`)
          .join(', ');
        code += '\n';
        if (funcSymbol.node.external) {
          code += `declare i32 @${ funcSymbol.name }(${ args })\n`;
        } else {
          code += `define i32 @${ funcSymbol.name }(${ args }) {\n`;
          for (const [blockId, block] of f.blocks) {
            if (blockId != 'entry') {
              code += '\n';
            }
            code += `${blockId}:\n`;
            // エントリブロックの最初でスタックを確保
            if (blockId == 'entry') {
              for (const inst of f.allocationArea) {
                code += `  ${inst}\n`;
              }
            }
            for (const inst of block.instructions) {
              code += `  ${inst}\n`;
            }
          }
          code += '}\n';
        }
        break;
      }
      case 'VariableDeclNode': {
        const symbol = unitSymbol.nameTable.get(decl.name)! as VariableSymbol;
        // TODO
        break;
      }
    }
  }
  return code;
}

/*
emitInstruction関数の戻り値のEmitResultを使って親ノードに情報を伝えることができます。
そのノードが値を返すのかどうか、最後にreturnするのかどうか、最後にbreakするのかどうか、など。
例えば、親ノード側で最後にret命令の挿入が必要なのかを判断する際に利用できます。
*/

function emitInstruction(
  f: FunctionContext,
  node: SyntaxNode,
  unitSymbol: UnitSymbol,
  funcSymbol: FunctionSymbol,
  loop: { loopEntryBlock: string, loopEndBlock: string } | undefined,
): EmitResult {
  switch (node.kind) {
    //case 'FuncParameterNode':
    case 'FunctionDeclNode': {
      // externalの場合はemitしない
      if (node.external) {
        return ['none'];
      }

      // add entry block
      const entryBlock = f.createBlock('entry');
      f.currentBlock = entryBlock;

      for (const parameter of node.parameters) {
        const variableSymbol = unitSymbol.nodeTable.get(parameter)! as VariableSymbol;
        const ptrReg = f.createLocalId(`${parameter.name}_ptr`);
        f.allocationArea.push(`%${ptrReg} = alloca i32`);
        f.writeInst(`store i32 %${parameter.name}, ptr %${ptrReg}`);
        // レジスタ名をシンボルに記憶
        variableSymbol.registerName = ptrReg;
      }

      let result;
      for (const step of node.body!) {
        result = emitInstruction(f, step, unitSymbol, funcSymbol, undefined);
        if (result[0] == 'return') {
          break;
        }
      }
      if (result != null) {
        if (result[0] == 'expr') {
          f.writeInst(`ret ${result[1]} ${result[2]}`);
        }
        if (result[0] == 'none') {
          f.writeInst(`ret void`);
        }
      }
      return ['none'];
    }
    // case 'TypeRefNode':
    case 'VariableDeclNode': {
      const variableSymbol = unitSymbol.nodeTable.get(node)! as VariableSymbol;
      const ptrReg = f.createLocalId(`${node.name}_ptr`);
      f.allocationArea.push(`%${ptrReg} = alloca i32`);
      // レジスタ名をシンボルに記憶
      variableSymbol.registerName = ptrReg;
      if (node.expr != null) {
        const result = emitInstruction(f, node.expr, unitSymbol, funcSymbol, loop);
        if (result[0] == 'expr') {
          f.writeInst(`store ${result[1]} ${result[2]}, ptr %${variableSymbol.registerName}`);
        }
        if (result[0] == 'return') {
          return result;
        }
      }
      return ['none'];
    }
    case 'BreakNode': {
      if (loop != null) {
        f.writeInst(`br label %${loop.loopEndBlock}`);
      }
      return ['break'];
    }
    case 'ContinueNode': {
      if (loop != null) {
        f.writeInst(`br label %${loop.loopEntryBlock}`);
      }
      return ['continue'];
    }
    case 'ReturnNode': {
      if (node.expr != null) {
        const result = emitInstruction(f, node.expr, unitSymbol, funcSymbol, loop);
        if (result[0] != 'expr') {
          throw new Error('expression expected');
        }
        f.writeInst(`ret ${result[1]} ${result[2]}`);
        return ['return'];
      } else {
        f.writeInst('ret void');
        return ['return'];
      }
    }
    case 'AssignNode': {
      const variableSymbol = unitSymbol.nodeTable.get(node.target)! as VariableSymbol;
      const result = emitInstruction(f, node.expr, unitSymbol, funcSymbol, loop);
      if (result[0] != 'expr') {
        throw new Error('expression expected');
      }
      f.writeInst(`store ${result[1]} ${result[2]}, ptr %${variableSymbol.registerName}`);
      return ['none'];
    }
    case 'WhileNode': {
      const condBlock = f.createBlockId('while.cond');
      const bodyBlock = f.createBlockId('while.body');
      const afterBlock = f.createBlockId('while.after');

      f.writeInst(`br label %${condBlock}`);

      // cond block
      f.currentBlock = f.createBlock(condBlock);

      const conditionResult = emitInstruction(f, node.expr, unitSymbol, funcSymbol, loop);
      if (conditionResult[0] != 'expr') {
        throw new Error('expression expected');
      }
      const conditionReg = f.createLocalId('while_cond');
      f.writeInst(`%${conditionReg} = icmp ne ${conditionResult[1]} ${conditionResult[2]}, 0`);
      f.writeInst(`br i1 %${conditionReg}, label %${bodyBlock}, label %${afterBlock}`);

      // body block
      f.currentBlock = f.createBlock(bodyBlock);

      let reachable = true;
      for (const step of node.body) {
        const loopInfo = { loopEntryBlock: condBlock, loopEndBlock: afterBlock };
        const result = emitInstruction(f, step, unitSymbol, funcSymbol, loopInfo);
        if (result[0] == 'return' || result[0] == 'break' || result[0] == 'continue') {
          reachable = false;
          break;
        }
      }

      if (reachable) {
        f.writeInst(`br label %${condBlock}`);
      }

      // after block
      f.currentBlock = f.createBlock(afterBlock);

      return ['none'];
    }
    case 'ExpressionStatementNode': {
      const result = emitInstruction(f, node.expr, unitSymbol, funcSymbol, loop);
      if (result[0] == 'return') {
        return result;
      }
      return ['none'];
    }
    case 'NumberLiteralNode': {
      return ['expr', 'i32', node.value.toString()];
    }
    case 'ReferenceNode': {
      const variableSymbol = unitSymbol.nodeTable.get(node)! as VariableSymbol;
      const valueReg = f.createLocalId(`${node.name}_val`);
      const type = 'i32';
      f.writeInst(`%${valueReg} = load ${type}, ptr %${variableSymbol.registerName}`);
      return ['expr', type, `%${valueReg}`];
    }
    case 'BinaryNode': {
      const leftResult = emitInstruction(f, node.left, unitSymbol, funcSymbol, loop);
      const rightResult = emitInstruction(f, node.right, unitSymbol, funcSymbol, loop);
      if (leftResult[0] != 'expr') {
        throw new Error('expression expected');
      }
      if (rightResult[0] != 'expr') {
        throw new Error('expression expected');
      }
      switch (node.mode) {
        case 'add':
        case 'sub':
        case 'mul':
        case 'div':
        case 'rem':
        case 'bitand':
        case 'bitor':
        case 'xor':
        case 'shl':
        case 'shr': {
          let inst;
          if (node.mode == 'rem') inst = 'srem';
          else if (node.mode == 'bitand') inst = 'and';
          else if (node.mode == 'bitor') inst = 'or';
          else if (node.mode == 'shr') inst = 'ashr';
          else inst = node.mode;
          const reg = f.createLocalId('op');
          f.writeInst(`%${reg} = ${inst} ${leftResult[1]} ${leftResult[2]}, ${rightResult[2]}`);
          return ['expr', leftResult[1], `%${reg}`];
        }
        case 'eq':
        case 'neq':
        case 'gt':
        case 'gte':
        case 'lt':
        case 'lte': {
          let mode;
          if (node.mode == 'neq') mode = 'ne';
          else if (node.mode == 'gt') mode = 'sgt';
          else if (node.mode == 'gte') mode = 'sge';
          else if (node.mode == 'lt') mode = 'slt';
          else if (node.mode == 'lte') mode = 'sle';
          else mode = node.mode;
          const reg = f.createLocalId('op');
          f.writeInst(`%${reg} = icmp ${mode} ${leftResult[1]} ${leftResult[2]}, ${rightResult[2]}`);
          return ['expr', 'i1', `%${reg}`];
        }
        case 'and':
        case 'or': {
          let inst;
          inst = node.mode;
          const reg = f.createLocalId('op');
          f.writeInst(`%${reg} = ${inst} ${leftResult[1]} ${leftResult[2]}, ${rightResult[2]}`);
          return ['expr', leftResult[1], `%${reg}`];
        }
      }
      throw new Error('unsupported operation mode');
    }
    case 'UnaryNode': {
      const result = emitInstruction(f, node.expr, unitSymbol, funcSymbol, loop);
      if (result[0] != 'expr') {
        throw new Error('expression expected');
      }
      switch (node.mode) {
        case 'minus': {
          const reg = f.createLocalId('op');
          f.writeInst(`%${reg} = sub ${result[1]} 0, ${result[2]}`);
          return ['expr', result[1], `%${reg}`];
        }
        case 'not': {
          const reg = f.createLocalId('op');
          f.writeInst(`%${reg} = icmp eq ${result[1]} ${result[2]}, 0`);
          return ['expr', 'i1', `%${reg}`];
        }
        case 'compl': {
          const reg = f.createLocalId('op');
          f.writeInst(`%${reg} = xor ${result[1]} ${result[2]}, -1`);
          return ['expr', result[1], `%${reg}`];
        }
        case 'plus': {
          return result;
        }
      }
      throw new Error('unsupported operation mode');
    }
    case 'IfNode': {
      const conditionResult = emitInstruction(f, node.cond, unitSymbol, funcSymbol, loop);
      if (conditionResult[0] != 'expr') {
        throw new Error('expression expected');
      }

      const thenBlock = f.createBlockId('if.then');
      const elseBlock = f.createBlockId('if.else');
      const endBlock = f.createBlockId('if.end');

      const conditionReg = f.createLocalId('cond');
      f.writeInst(`%${conditionReg} = icmp ne ${conditionResult[1]} ${conditionResult[2]}, 0`);
      f.writeInst(`br i1 %${conditionReg}, label %${thenBlock}, label %${elseBlock}`);

      const brPtrReg = f.createLocalId('br_ptr');
      f.allocationArea.push(`%${brPtrReg} = alloca i32`);

      // then block
      let reachableThen = true;
      f.currentBlock = f.createBlock(thenBlock);
      const thenResult = emitInstruction(f, node.thenExpr, unitSymbol, funcSymbol, loop);
      if (thenResult[0] == 'expr') {
        f.writeInst(`store ${thenResult[1]} ${thenResult[2]}, ptr %${brPtrReg}`);
        f.writeInst(`br label %${endBlock}`);
      } else if (thenResult[0] == 'none') {
        f.writeInst(`br label %${endBlock}`);
      } else {
        reachableThen = false;
      }

      // else block
      let reachableElse = true;
      f.currentBlock = f.createBlock(elseBlock);
      if (node.elseExpr != null) {
        const elseResult = emitInstruction(f, node.elseExpr, unitSymbol, funcSymbol, loop);
        if (elseResult[0] == 'expr') {
          f.writeInst(`store ${elseResult[1]} ${elseResult[2]}, ptr %${brPtrReg}`);
          f.writeInst(`br label %${endBlock}`);
        } else if (elseResult[0] == 'none') {
          f.writeInst(`br label %${endBlock}`);
        } else {
          reachableElse = false;
        }
      } else {
        f.writeInst(`br label %${endBlock}`);
      }

      if (reachableThen || reachableElse) {
        // end block
        f.currentBlock = f.createBlock(endBlock);
        const brReg = f.createLocalId('br_val');
        f.writeInst(`%${brReg} = load i32, ptr %${brPtrReg}`);

        return ['expr', 'i32', `%${brReg}`];
      } else {
        return ['return'];
      }
    }
    // case 'SwitchNode':
    case 'BlockNode': {
      let lastExpr;
      for (let i = 0; i < node.body.length; i++) {
        const step = node.body[i];
        const result = emitInstruction(f, step, unitSymbol, funcSymbol, loop);
        if (result[0] == 'return') {
          return result;
        }
        if (result[0] == 'expr' && i == node.body.length - 1) {
          lastExpr = result;
        }
      }
      if (lastExpr != null) {
        return lastExpr;
      } else {
        return ['none'];
      }
    }
    case 'CallNode': {
      const args: EmitResult[] = [];
      for (let i = 0; i < node.args.length; i++) {
        const argResult = emitInstruction(f, node.args[i], unitSymbol, funcSymbol, loop);
        args.push(argResult);
      }

      const exprSymbol = unitSymbol.nodeTable.get(node.expr)! as FunctionSymbol;
      const resultReg = f.createLocalId(`call_ret`);
      f.writeInst(`%${resultReg} = call i32 @${exprSymbol.name}(${args.map(x => `${x[1]} ${x[2]}`).join(', ')})`);

      return ['expr', 'i32', `%${resultReg}`];
    }
  }
  throw new Error('emit llvm failure');
}

type EmitResult =
  | ['none']
  | ['return']
  | ['break']
  | ['continue']
  | ['expr', string, string]; // type, operand

class FunctionContext {
  blocks: Map<string, BasicBlock> = new Map();
  currentBlock: BasicBlock | undefined;
  allocationArea: string[] = [];
  private localIdSet: Set<string> = new Set();
  private localIdCache: { name: string, index: number } | undefined;
  private blockIdSet: Set<string> = new Set();
  private blockIdCache: { name: string, index: number } | undefined;

  createLocalId(name: string): string {
    if (!this.localIdSet.has(name)) {
      this.localIdSet.add(name);
      return name;
    }
    let index = 0;
    if (this.localIdCache?.name == name) {
      index = this.localIdCache.index + 1;
    }
    while (this.localIdSet.has(name + index)) {
      index++;
    }
    this.localIdSet.add(name + index);
    this.localIdCache = { name, index };
    return name + index;
  }

  createBlockId(name: string): string {
    if (!this.blockIdSet.has(name)) {
      this.blockIdSet.add(name);
      return name;
    }
    let index = 0;
    if (this.blockIdCache?.name == name) {
      index = this.blockIdCache.index + 1;
    }
    while (this.blockIdSet.has(name + index)) {
      index++;
    }
    this.blockIdSet.add(name + index);
    this.blockIdCache = { name, index };
    return name + index;
  }

  createBlock(blockId: string): BasicBlock {
    let block = new BasicBlock(blockId);
    this.blocks.set(blockId, block);
    return block;
  }

  writeInst(inst: string) {
    if (this.currentBlock == null) return;
    this.currentBlock.instructions.push(inst);
  }
}

class BasicBlock {
  instructions: string[] = [];

  constructor(
    public blockId: string
  ) {}
}
