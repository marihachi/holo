import { SyntaxNode, Unit } from '../syntax/node.js';

export class Emitter {
  code: string;
  indentLevel: number;

  constructor() {
    this.code = '';
    this.indentLevel = 0;
  }

  beginLine() {
    this.code += '  '.repeat(this.indentLevel);
  }

  endLine() {
    this.code += '\n';
  }

  level(levelOffset: number) {
    this.indentLevel += levelOffset;
  }
}

export function generateCode(node: Unit) {
  const e = new Emitter();
  emit(e, node);
  return e.code;
}

function emit(e: Emitter, node: SyntaxNode) {
  switch (node.kind) {
    case 'Unit': {
      for (const decl of node.decls) {
        e.beginLine();
        emit(e, decl);
        e.endLine();
      }
      break;
    }
    case 'FunctionDecl': {
      e.code += 'int ';
      e.code += node.name;
      e.code += '(';
      for (let i = 0; i < node.paramNames.length; i++) {
        if (i > 0) e.code += ', ';
        e.code += `int ${node.paramNames[i]}`;
      }
      e.code += ')';
      e.code += ' {';
      e.endLine();

      e.level(1);
      for (const step of node.body) {
        e.beginLine();
        emit(e, step);
        e.endLine();
      }
      e.level(-1);

      e.beginLine();
      e.code += '}';
      break;
    }
    case 'VariableDecl': {
      e.code += `int ${node.name}`;
      if (node.body != null) {
        e.endLine();
        e.level(1);
        e.beginLine();
        e.code += '= ';
        emit(e, node.body);
        e.code += ';';
        e.level(-1);
      } else {
        e.code += ';';
      }
      break;
    }
    case 'NumberLiteral': {
      e.code += node.value;
      break;
    }
    case 'Reference': {
      e.code += node.name;
      break;
    }
    case 'Binary': {
      // TODO
      break;
    }
    case 'Unary': {
      // TODO
      break;
    }
    case 'If': {
      // TODO
      break;
    }
    case 'Block': {
      // TODO
      break;
    }
    case 'Break': {
      e.code += 'break';
      e.code += ';';
      break;
    }
    case 'Continue': {
      e.code += 'continue';
      e.code += ';';
      break;
    }
    case 'Return': {
      e.code += 'return';
      if (node.expr != null) {
        e.code += ' ';
        emit(e, node.expr);
      }
      e.code += ';';
      break;
    }
    case 'Assign': {
      emit(e, node.left);
      switch (node.mode) {
        case 'simple': { e.code += ' = '; break; }
        case 'add': { e.code += ' += '; break; }
        case 'sub': { e.code += ' -= '; break; }
        case 'mul': { e.code += ' *= '; break; }
        case 'div': { e.code += ' /= '; break; }
        case 'rem': { e.code += ' %= '; break; }
        case 'bitand': { e.code += ' &= '; break; }
        case 'bitor': { e.code += ' |= '; break; }
        case 'shl': { e.code += ' <<= '; break; }
        case 'shr': { e.code += ' >>= '; break; }
      }
      emit(e, node.right);
      break;
    }
    case 'While': {
      e.code += 'while ';
      e.code += '(';
      emit(e, node.cond);
      e.code += ')';
      e.code += ' {';
      e.endLine();

      e.level(1);
      for (const step of node.body) {
        e.beginLine();
        emit(e, step);
        e.endLine();
      }
      e.level(-1);

      e.beginLine();
      e.code += '}';
      break;
    }
    case 'ExpressionStatement': {
      emit(e, node.expr);
      e.code += ';';
      break;
    }
  }
}
