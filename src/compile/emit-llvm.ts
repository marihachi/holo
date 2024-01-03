import { FunctionDeclNode, UnitNode } from './syntax-node.js';

class FunctionContext {
  basicBlocks: Map<number, BasicBlock> = new Map();
  nextBlockId: number = 0;

  createBlock(): BasicBlock {
    let block = new BasicBlock();
    this.basicBlocks.set(this.nextBlockId, block);
    this.nextBlockId += 1;
    return block;
  }
}

class BasicBlock {
  instructions: number[] = [];
}

export function emit(syntaxNode: UnitNode) {
  for (const decl of syntaxNode.decls) {
    switch (decl.kind) {
      case 'FunctionDeclNode': {
        const f = new FunctionContext();
        emitFunction(f, decl);
        break;
      }
    }
  }
}

function emitFunction(f: FunctionContext, syntaxNode: FunctionDeclNode): string {
  let code: string = '';
  code += `define @${ syntaxNode.name }{\n`;
  // syntaxNode.body
  code += '}\n';
  return code;
}
