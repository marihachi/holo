import { FunctionSymbol, UnitSymbol } from './semantic-node.js';
import { SyntaxNode, isExpressionNode } from './syntax-node.js';

// 関数の最後の式をreturnする処理が必要

class FunctionContext {
  blocks: Map<string, BasicBlock> = new Map();
  entryBlock: BasicBlock | undefined;
  currentBlock: BasicBlock | undefined;
  private nextBlockId: number = 1;
  private nextLocalId: number = 0;

  createBlockId(): string {
    const id = `b${this.nextBlockId}`;
    this.nextBlockId += 1;
    return id;
  }

  createLocalId(): string {
    const id = `L${this.nextLocalId}`;
    this.nextLocalId += 1;
    return id;
  }

  createBlock(blockId?: string): BasicBlock {
    if (blockId == null) {
      blockId = this.createBlockId();
    }
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
  code += 'target triple = "x86_64-unknown-linux-gnu"\n\n';

  for (const decl of unitSymbol.node.decls) {
    switch (decl.kind) {
      case 'FunctionDeclNode': {
        const funcSymbol = unitSymbol.nameTable.get(decl.name)! as FunctionSymbol;
        const f = new FunctionContext();
        code += emitFunction(f, funcSymbol);
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
    emitInstruction(f, step, undefined);
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
function emitInstruction(f: FunctionContext, node: SyntaxNode, retVarId: string | undefined): { type: string, value: string } | undefined {
  switch (node.kind) {
    case 'VariableDeclNode': {
      // TODO
      return;
    }
    case 'AssignNode': {
      // TODO
      return;
    }
    case 'ReturnNode': {
      if (node.expr != null) {
        const expr = emitInstruction(f, node.expr, undefined);
        f.writeInst(`ret ${expr.type} ${expr.value}`);
      } else {
        f.writeInst('ret void');
      }
      return;
    }

    case 'NumberLiteralNode': {
      return { type: 'i32', value: node.value.toString() };
    }
    case 'BinaryNode': {
      const left = emitInstruction(f, node.left, undefined);
      const right = emitInstruction(f, node.right, undefined);
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
      const localId = f.createLocalId();
      f.writeInst(`%${localId} = ${inst} i32 ${left.value}, ${right.value}`);
      return { type: 'i32', value: `%${localId}` };
    }
    case 'IfNode': {
      const cond = emitInstruction(f, node.cond, undefined);

      const thenBlockId = f.createBlockId();
      const elseBlockId = f.createBlockId();
      const contBlockId = f.createBlockId();

      f.writeInst(`br i1 ${cond.value}, label %${thenBlockId}, label %${elseBlockId}`);

      const retId = f.createLocalId();
      f.entryBlock!.stackAlloc.push({ name: retId, type: 'i32' });

      f.currentBlock = f.createBlock(thenBlockId);
      if (node.thenExpr.kind == 'BlockNode') {
        emitInstruction(f, node.thenExpr, retId);
      } else {
        const expr = emitInstruction(f, node.thenExpr, undefined);
        f.writeInst(`store ${expr!.type} ${expr!.value}, ptr %${retId}`);
      }
      f.writeInst(`br label %${contBlockId}`);

      f.currentBlock = f.createBlock(elseBlockId);
      if (node.elseExpr.kind == 'BlockNode') {
        emitInstruction(f, node.elseExpr, retId);
      } else {
        const expr = emitInstruction(f, node.elseExpr, undefined);
        f.writeInst(`store ${expr!.type} ${expr!.value}, ptr %${retId}`);
      }
      f.writeInst(`br label %${contBlockId}`);

      f.currentBlock = f.createBlock(contBlockId);

      return { type: 'i32', value: `%${retId}` };
    }
    case 'BlockNode': {
      for (let i = 0; i < node.body.length; i++) {
        const step = node.body[i];
        const expr = emitInstruction(f, step, undefined);
        if (isExpressionNode(step) && i == node.body.length - 1) {
          f.writeInst(`store ${expr!.type} ${expr!.value}, ptr %${retVarId}`);
        }
      }
      return { type: 'i32', value: `%${retVarId}` };
    }
  }
  throw new Error('generate code failure');
}
