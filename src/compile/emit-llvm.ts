import { FunctionSymbol, UnitSymbol } from './semantic-node.js';
import { SyntaxNode } from './syntax-node.js';

// 式の最後でもreturnしてしまう問題がある
// desugarFuncReturnExprがまずそう
// AST変形は面倒なため、コード生成でうまくやれないか

class FunctionContext {
  blocks: Map<string, BasicBlock> = new Map();
  nextBlockId: number = 1;
  currentBlock: BasicBlock | undefined;
  nextLocalId: number = 0;

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
    let block = new BasicBlock();
    this.blocks.set(blockId, block);
    return block;
  }

  setCurrentBlock(block: BasicBlock): void {
    this.currentBlock = block;
  }

  writeInst(inst: string) {
    this.currentBlock?.instructions.push(inst);
  }
}

class BasicBlock {
  instructions: string[] = [];
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
  const startBlock = f.createBlock('entry');
  f.setCurrentBlock(startBlock);

  for (const step of funcSymbol.node.body) {
    emitInstruction(f, step, undefined);
  }

  // emit code
  let code = '';
  code += `define i32 @${ funcSymbol.name }() {\n`;
  for (const [blockId, block] of f.blocks) {
    code += `${blockId}:\n`;
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

      f.writeInst(`br i1 ${cond.value}, label %${thenBlockId}, %${elseBlockId}`);

      const retVarId = f.createLocalId();

      f.setCurrentBlock(f.createBlock(thenBlockId));
      emitInstruction(f, node.thenExpr, retVarId);
      f.writeInst(`br label %${contBlockId}`);

      f.setCurrentBlock(f.createBlock(elseBlockId));
      emitInstruction(f, node.elseExpr, retVarId);
      f.writeInst(`br label %${contBlockId}`);

      f.setCurrentBlock(f.createBlock(contBlockId));

      return { type: 'i32', value: `%${retVarId}` };
    }
    case 'BlockNode': {
      for (const step of node.body) {
        emitInstruction(f, step, retVarId);
      }
      return { type: 'i32', value: `%${retVarId}` };
    }
  }
  throw new Error('generate code failure');
}
