import { FunctionSymbol, UnitSymbol, VariableSymbol } from './semantic-node.js';
import { ReferenceNode, SyntaxNode, isExpressionNode } from './syntax-node.js';

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
        code += '\n' + emitFunction(f, funcSymbol);
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

function emitFunction(f: FunctionContext, funcSymbol: FunctionSymbol): string {
  // add entry block
  const entryBlock = f.createBlock('entry');
  f.entryBlock = entryBlock;
  f.currentBlock = entryBlock;

  for (const step of funcSymbol.node.body) {
    emitInstruction(f, step, undefined, funcSymbol);
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
function emitInstruction(f: FunctionContext, node: SyntaxNode, parentPtrId: string | undefined, funcSymbol: FunctionSymbol): { type: string, value: string } | undefined {
  switch (node.kind) {
    case 'VariableDeclNode': {
      const stackMemId = f.createLocalId('p');
      f.entryBlock?.stackAlloc.push({ name: stackMemId, type: 'i32' });
      const variableSymbol = funcSymbol.nameTable.get(node.name)! as VariableSymbol;
      variableSymbol.registerName = stackMemId;
      if (node.expr != null) {
        const expr = emitInstruction(f, node.expr, undefined, funcSymbol);
        f.writeInst(`store ${expr!.type} ${expr!.value}, ptr %${stackMemId}`);
      }
      return;
    }
    case 'AssignNode': {
      const targetName = (node.target as ReferenceNode).name;
      const variableSymbol = funcSymbol.nameTable.get(targetName)! as VariableSymbol;
      const expr = emitInstruction(f, node.expr, undefined, funcSymbol);
      f.writeInst(`store ${expr!.type} ${expr!.value}, ptr %${variableSymbol.registerName}`);
      return;
    }
    case 'ReturnNode': {
      if (node.expr != null) {
        const expr = emitInstruction(f, node.expr, undefined, funcSymbol);
        f.writeInst(`ret ${expr!.type} ${expr!.value}`);
      } else {
        f.writeInst('ret void');
      }
      return;
    }

    case 'NumberLiteralNode': {
      return { type: 'i32', value: node.value.toString() };
    }
    case 'ReferenceNode': {
      const symbol = funcSymbol.nameTable.get(node.name)! as VariableSymbol;
      const refValue = f.createLocalId('ref_v');
      f.writeInst(`%${refValue} = load i32, ptr %${symbol.registerName}`);
      return { type: 'i32', value: `%${refValue}` };
    }
    case 'BinaryNode': {
      const left = emitInstruction(f, node.left, undefined, funcSymbol);
      const right = emitInstruction(f, node.right, undefined, funcSymbol);
      let inst;
      switch (node.mode) {
        case 'add': inst = node.mode; break;
        case 'sub': inst = node.mode; break;
        case 'mul': inst = node.mode; break;
        case 'div': inst = 'sdiv'; break;
        case 'rem': inst = 'srem'; break;
        case 'eq': inst = 'icmp eq'; break;
        case 'neq': inst = 'icmp ne'; break;
        case 'gt': inst = 'icmp sgt'; break;
        case 'gte': inst = 'icmp sge'; break;
        case 'lt': inst = 'icmp slt'; break;
        case 'lte': inst = 'icmp sle'; break;
        default: {
          throw new Error('unsupported operation mode');
        }
      }
      const localId = f.createLocalId('op');
      f.writeInst(`%${localId} = ${inst} i32 ${left!.value}, ${right!.value}`);
      return { type: 'i32', value: `%${localId}` };
    }
    case 'IfNode': {
      const cond = emitInstruction(f, node.cond, undefined, funcSymbol);

      const thenBlockId = f.createBlockId('then');
      const elseBlockId = f.createBlockId('else');
      const contBlockId = f.createBlockId('cont');

      f.writeInst(`br i1 ${cond!.value}, label %${thenBlockId}, label %${elseBlockId}`);

      const stackMemId = f.createLocalId('p');
      f.entryBlock?.stackAlloc.push({ name: stackMemId, type: 'i32' });

      f.currentBlock = f.createBlock(thenBlockId);
      if (node.thenExpr.kind == 'BlockNode') {
        emitInstruction(f, node.thenExpr, stackMemId, funcSymbol);
      } else {
        const expr = emitInstruction(f, node.thenExpr, undefined, funcSymbol);
        f.writeInst(`store ${expr!.type} ${expr!.value}, ptr %${stackMemId}`);
      }
      f.writeInst(`br label %${contBlockId}`);

      f.currentBlock = f.createBlock(elseBlockId);
      if (node.elseExpr != null) {
        if (node.elseExpr.kind == 'BlockNode') {
          emitInstruction(f, node.elseExpr, stackMemId, funcSymbol);
        } else {
          const expr = emitInstruction(f, node.elseExpr, undefined, funcSymbol);
          f.writeInst(`store ${expr!.type} ${expr!.value}, ptr %${stackMemId}`);
        }
      }
      f.writeInst(`br label %${contBlockId}`);

      f.currentBlock = f.createBlock(contBlockId);

      const retId2 = f.createLocalId('if_r');
      f.writeInst(`%${retId2} = load i32, ptr %${stackMemId}`);

      return { type: 'i32', value: `%${retId2}` };
    }
    case 'BlockNode': {
      let stackMemId;
      if (parentPtrId != null) {
        stackMemId = parentPtrId;
      } else {
        stackMemId = f.createLocalId('p');
        f.entryBlock?.stackAlloc.push({ name: stackMemId, type: 'i32' });
      }

      for (let i = 0; i < node.body.length; i++) {
        const step = node.body[i];
        const expr = emitInstruction(f, step, undefined, funcSymbol);
        if (isExpressionNode(step) && i == node.body.length - 1) {
          f.writeInst(`store ${expr!.type} ${expr!.value}, ptr %${stackMemId}`);
        }
      }

      if (parentPtrId != null) {
        return { type: 'i32', value: `%${stackMemId}` };
      } else {
        const retId = f.createLocalId('blck_r');
        f.writeInst(`%${retId} = load i32, ptr %${stackMemId}`);
        return { type: 'i32', value: `%${retId}` };
      }
    }
  }
  throw new Error('generate code failure');
}
