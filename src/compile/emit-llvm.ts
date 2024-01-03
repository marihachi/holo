import { ExpressionNode, FunctionDeclNode, StatementNode, UnitNode } from './syntax-node.js';

class FunctionContext {
  blocks: Map<string, BasicBlock> = new Map();
  nextBlockId: number = 1;
  currentBlock: BasicBlock | undefined;

  createBlockId(): string {
    const id = `b${this.nextBlockId}`;
    this.nextBlockId += 1;
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
}

class BasicBlock {
  instructions: string[] = [];
}

export function emit(syntaxNode: UnitNode): string {
  let code = '';
  for (const decl of syntaxNode.decls) {
    switch (decl.kind) {
      case 'FunctionDeclNode': {
        const f = new FunctionContext();
        code += emitFunction(f, decl);
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

function emitFunction(f: FunctionContext, syntaxNode: FunctionDeclNode): string {
  // add entry block
  const startBlock = f.createBlock('entry');
  f.setCurrentBlock(startBlock);

  for (const step of syntaxNode.body) {
    visitStepForMakeBlock(f, step);
  }

  // emit code
  let code = '';
  code += `define @${ syntaxNode.name }{\n`;
  for (const [blockId, block] of f.blocks) {
    code += `${blockId}:\n`;
    for (const inst of block.instructions) {
      code += `  ${inst}\n`;
    }
  }
  code += '}\n';

  return code;
}

function visitStepForMakeBlock(f: FunctionContext, syntaxNode: ExpressionNode | StatementNode): void {
  // TODO
}
