import { SyntaxNode, Unit } from '../syntax/node.js';

export class Emitter {
  code: string;

  constructor() {
    this.code = '';
  }

  append(code: string) {
    this.code += code;
  }
}

export function generateCode(e: Emitter, node: Unit) {
  emit(e, node);
}

function emit(e: Emitter, node: SyntaxNode) {
  switch (node.kind) {
    case 'Unit': {
      for (const decl of node.decls) {
        emit(e, decl);
      }
      break;
    }
    case 'FunctionDecl': {
      e.append(`int ${node.name}(`);
      for (let i = 0; i < node.paramNames.length; i++) {
        if (i > 0) {
          e.append(', ');
        }
        e.append(`int ${node.paramNames[i]}`);
      }
      e.append(`) {\n`);
      for (const step of node.body) {
        emit(e, step);
      }
      e.append('}\n');
      break;
    }
    case 'VariableDecl': {
      e.append(`int ${node.name}`);
      if (node.body != null) {
        e.append(' = ');
        emit(e, node.body);
      }
      e.append(';\n');
      break;
    }
    case 'ExpressionStatement': {
      emit(e, node.expr);
      e.append(';\n');
      break;
    }
    case 'NumberLiteral': {
      e.append(node.value.toString());
      break;
    }
  }
}
