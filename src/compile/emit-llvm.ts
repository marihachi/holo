import { FunctionSymbol, UnitSymbol } from './semantic-node.js';
import { ExpressionNode, StatementNode } from './syntax-node.js';

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
    const id = `${this.nextLocalId}`;
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

  makeFnInstructions(f, funcSymbol.node.body);

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
 * 関数の命令列を生成する。
*/
function makeFnInstructions(f: FunctionContext, body: (ExpressionNode | StatementNode)[]): void {
  for (const step of body) {
    switch (step.kind) {
      case 'VariableDeclNode': {
        // TODO
        break;
      }
      case 'AssignNode': {
        // TODO
        break;
      }
      case 'ReturnNode': {
        if (step.expr != null) {
          const { type, value } = emitExprInReturn(f, step.expr);
          f.writeInst(`ret ${type} ${value}`);
        } else {
          f.writeInst('ret void');
        }
        break;
      }
    }
  }
}

/**
 * 式ノードを解析して式全体としての値を返す。  
 * 返される式が単純な値ではない場合は値を返すための命令列を生成する。
*/
function emitExprInReturn(f: FunctionContext, expr: ExpressionNode): { type: string, value: string } {
  switch (expr.kind) {
    case 'NumberLiteralNode': {
      return { type: 'i32', value: expr.value.toString() };
    }
    case 'BinaryNode': {
      const left = emitExprInReturn(f, expr.left);
      const right = emitExprInReturn(f, expr.right);

      let inst;
      switch (expr.mode) {
        case 'add': inst = expr.mode; break;
        case 'sub': inst = expr.mode; break;
        case 'mul': inst = expr.mode; break;
        case 'div': inst = 'sdiv'; break;
        case 'rem': inst = 'srem'; break;
        default: {
          throw new Error('unsupported operation mode');
        }
      }

      const localId = f.createLocalId();
      f.writeInst(`%${localId} = ${inst} i32 ${left.value}, ${right.value}`);
      return { type: 'i32', value: `%${localId}` };
    }
  }
  throw new Error('generate code failure');
}
