import { FunctionSymbol, UnitSymbol, VariableSymbol } from './semantic-node.js';
import { SyntaxNode, isExpressionNode } from './syntax-node.js';

// 関数の最後の式をreturnする処理が必要

class FunctionContext {
  blocks: Map<string, BasicBlock> = new Map();
  entryBlock: BasicBlock | undefined;
  currentBlock: BasicBlock | undefined;
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
  stackAlloc: { name: string, type: string }[] = [];
  instructions: string[] = [];

  constructor(
    public blockId: string
  ) {}
}

export function emit(unitSymbol: UnitSymbol): string {
  let code = '';

  // setup ISA
  code += 'target triple = "x86_64-unknown-linux-gnu"\n';

  for (const decl of unitSymbol.node.decls) {
    switch (decl.kind) {
      case 'FunctionDeclNode': {
        const funcSymbol = unitSymbol.nameTable.get(decl.name)! as FunctionSymbol;
        const f = new FunctionContext();
        code += '\n' + emitFunction(f, unitSymbol, funcSymbol);
        break;
      }
      case 'VariableDeclNode': {
        // TODO
        break;
      }
    }
  }
  return code;
}

function emitFunction(f: FunctionContext, unitSymbol: UnitSymbol, funcSymbol: FunctionSymbol): string {
  // add entry block
  const entryBlock = f.createBlock('entry');
  f.entryBlock = entryBlock;
  f.currentBlock = entryBlock;

  for (const step of funcSymbol.node.body) {
    emitInstruction(f, step, unitSymbol, funcSymbol);
  }

  // emit code
  let code = '';
  code += `define i32 @${ funcSymbol.name }() {\n`;
  for (const [blockId, block] of f.blocks) {
    code += `${blockId}:\n`;
    for (const local of block.stackAlloc) {
      code += `  %${local.name} = alloca ${local.type}\n`;
    }
    for (const inst of block.instructions) {
      code += `  ${inst}\n`;
    }
  }
  code += '}\n';

  return code;
}

/**
 * 命令を生成する。
*/
function emitInstruction(f: FunctionContext, node: SyntaxNode, unitSymbol: UnitSymbol, funcSymbol: FunctionSymbol): [] | [string] | [string, string] {
  switch (node.kind) {
    case 'VariableDeclNode': {
      const variableSymbol = unitSymbol.nodeTable.get(node)! as VariableSymbol;
      // allocaで確保したスタック領域を参照するレジスタ名
      const stackMemId = f.createLocalId('decl_p');
      // エントリブロック上にalloca命令を生成する
      f.entryBlock?.stackAlloc.push({ name: stackMemId, type: 'i32' });
      // レジスタ名をシンボルに記憶
      variableSymbol.registerName = stackMemId;
      if (node.expr != null) {
        const value = emitInstruction(f, node.expr, unitSymbol, funcSymbol);
        f.writeInst(`store ${value.join(' ')}, ptr %${variableSymbol.registerName}`);
      }
      return [];
    }
    case 'NumberLiteralNode': {
      return ['i32', node.value.toString()];
    }
    case 'ReferenceNode': {
      const variableSymbol = unitSymbol.nodeTable.get(node)! as VariableSymbol;
      const refValue = f.createLocalId('ref_v');
      f.writeInst(`%${refValue} = load i32, ptr %${variableSymbol.registerName}`);
      return ['i32', `%${refValue}`];
    }
    // case 'TypeRefNode': {
    //   break;
    // }
    case 'BinaryNode': {
      const leftValue = emitInstruction(f, node.left, unitSymbol, funcSymbol);
      const rightValue = emitInstruction(f, node.right, unitSymbol, funcSymbol);
      switch (node.mode) {
        case 'add':
        case 'sub':
        case 'mul':
        case 'div':
        case 'rem': {
          let inst;
          if (node.mode == 'rem') inst = 'srem';
          else inst = node.mode;
          const localId = f.createLocalId('op');
          f.writeInst(`%${localId} = ${inst} i32 ${leftValue[1]}, ${rightValue[1]}`);
          return ['i32', `%${localId}`];
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
          const localId = f.createLocalId('op');
          f.writeInst(`%${localId} = icmp ${mode} i32 ${leftValue[1]}, ${rightValue[1]}`);
          return ['i32', `%${localId}`];
        }
        // case 'and':
        // case 'or':
        // case 'xor':
        // case 'shl':
        // case 'shr':
        // case 'bitand':
        // case 'bitor': {
        //   return [];
        // }
      }
      throw new Error('unsupported operation mode');
    }
    case 'UnaryNode': {
      const value = emitInstruction(f, node.expr, unitSymbol, funcSymbol);
      switch (node.mode) {
        case 'minus': {
          const localId = f.createLocalId('op');
          f.writeInst(`%${localId} = sub i32 0, ${value[1]}`);
          return ['i32', `%${localId}`];
        }
        case 'not': {
          const localId = f.createLocalId('op');
          f.writeInst(`%${localId} = icmp ne ${value[1]}, 0`);
          return ['i1', `%${localId}`];
        }
        case 'compl': {
          const localId = f.createLocalId('op');
          f.writeInst(`%${localId} = xor i32 ${value[1]}, -1`);
          return ['i32', `%${localId}`];
        }
        case 'plus': {
          return value;
        }
      }
      throw new Error('unsupported operation mode');
    }
    case 'IfNode': {
      const condValue = emitInstruction(f, node.cond, unitSymbol, funcSymbol);

      const thenBlockId = f.createBlockId('then');
      const elseBlockId = f.createBlockId('else');
      const contBlockId = f.createBlockId('cont');

      f.writeInst(`br i1 ${condValue[1]}, label %${thenBlockId}, label %${elseBlockId}`);

      const storePtr = f.createLocalId('if_p');
      f.entryBlock?.stackAlloc.push({ name: storePtr, type: 'i32' });

      f.currentBlock = f.createBlock(thenBlockId);
      const thenValue = emitInstruction(f, node.thenExpr, unitSymbol, funcSymbol);
      f.writeInst(`store ${thenValue.join(' ')}, ptr %${storePtr}`);
      f.writeInst(`br label %${contBlockId}`);

      f.currentBlock = f.createBlock(elseBlockId);
      if (node.elseExpr != null) {
        const elseValue = emitInstruction(f, node.elseExpr, unitSymbol, funcSymbol);
        f.writeInst(`store ${elseValue.join(' ')}, ptr %${storePtr}`);
      }
      f.writeInst(`br label %${contBlockId}`);

      f.currentBlock = f.createBlock(contBlockId);

      const loadValue = f.createLocalId('if_r');
      f.writeInst(`%${loadValue} = load i32, ptr %${storePtr}`);

      return ['i32', `%${loadValue}`];
    }
    case 'BlockNode': {
      let lastValue;
      for (let i = 0; i < node.body.length; i++) {
        const step = node.body[i];
        const value = emitInstruction(f, step, unitSymbol, funcSymbol);
        if (isExpressionNode(step) && i == node.body.length - 1) {
          lastValue = value;
        }
      }
      if (lastValue != null) {
        return lastValue;
      } else {
        return ['void'];
      }
    }
    // case 'CallNode': {
    //   break;
    // }
    // case 'BreakNode': {
    //   break;
    // }
    // case 'ContinueNode': {
    //   break;
    // }
    case 'ReturnNode': {
      if (node.expr != null) {
        const value = emitInstruction(f, node.expr, unitSymbol, funcSymbol);
        f.writeInst(`ret ${value.join(' ')}`);
      } else {
        f.writeInst('ret void');
      }
      return [];
    }
    case 'AssignNode': {
      const variableSymbol = unitSymbol.nodeTable.get(node.target)! as VariableSymbol;
      const value = emitInstruction(f, node.expr, unitSymbol, funcSymbol);
      f.writeInst(`store ${value.join(' ')}, ptr %${variableSymbol.registerName}`);
      return [];
    }
    // case 'WhileNode': {
    //   break;
    // }
    // case 'SwitchNode': {
    //   break;
    // }
    case 'ExpressionStatementNode': {
      emitInstruction(f, node.expr, unitSymbol, funcSymbol);
      return [];
    }
  }
  throw new Error('generate code failure');
}
