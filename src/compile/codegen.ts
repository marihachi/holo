import { SyntaxNode, Unit, isExpression } from './ast.js';

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

export function generate(node: Unit) {
  const e = new Emitter();
  emit(e, node);
  return e.code;
}

function emit(e: Emitter, node: SyntaxNode, parent?: SyntaxNode) {
  switch (node.kind) {
    case 'Unit': {
      for (const decl of node.decls) {
        e.beginLine();
        emit(e, decl, node);
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
        emit(e, step, node);
        e.endLine();
      }
      e.level(-1);

      e.beginLine();
      e.code += '}';
      break;
    }
    case 'VariableDecl': {
      e.code += `int ${node.name}`;
      if (node.expr != null) {
        e.endLine();
        e.level(1);
        e.beginLine();
        e.code += '= ';
        emit(e, node.expr, node);
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
      switch (node.mode) {
        case "add": {
          emit(e, node.left, node);
          e.code += ' + ';
          emit(e, node.right, node);
        }
      }
      break;
    }
    case 'Unary': {
      // TODO
      break;
    }
    case 'If': {
      e.code += 'if (';
      emit(e, node.cond, node);
      e.code += ') ';
      emit(e, node.thenExpr, node);
      if (node.elseExpr) {
        e.code += ' else ';
        emit(e, node.elseExpr, node);
      }
      break;
    }
    case 'Block': {
      e.code += '{';
      e.level(1);
      for (const child of node.body) {
        e.endLine();
        e.beginLine();
        emit(e, child, node);
        e.code += ";";
      }
      e.endLine();
      e.level(-1);
      e.beginLine();
      e.code += '}';
      break;
    }
    case 'Call': {
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
        emit(e, node.expr, node);
      }
      e.code += ';';
      break;
    }
    case 'Assign': {
      emit(e, node.target, node);
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
      emit(e, node.expr, node);
      break;
    }
    case 'While': {
      e.code += 'while ';
      e.code += '(';
      emit(e, node.expr, node);
      e.code += ')';
      e.code += ' {';
      e.endLine();

      e.level(1);
      for (const step of node.body) {
        e.beginLine();
        emit(e, step, node);
        e.endLine();
      }
      e.level(-1);

      e.beginLine();
      e.code += '}';
      break;
    }
    case 'Switch': {
      e.code += 'switch ';
      e.code += '(';
      emit(e, node.expr, node);
      e.code += ')';
      e.code += ' {';
      e.endLine();

      e.level(1);
      for (const arm of node.arms) {
        e.beginLine();
        e.code += 'case ';
        emit(e, arm.cond, node);
        e.code += ': {';
        e.endLine();
        let blockResult;
        for (const step of arm.thenBlock.body) {
          if (!isExpression(step)) {
            e.beginLine();
            emit(e, step, arm.thenBlock);
            e.endLine();
          }
          blockResult = step;
        }
        // TODO: support Assign
        if (parent != null && blockResult != null && parent.kind == 'VariableDecl' && isExpression(blockResult)) {
          e.beginLine();
          e.code += `${parent.name} = `;
          emit(e, blockResult, arm.thenBlock);
          e.code += ';';
          e.endLine();
        }
        e.beginLine();
        e.code += '}';
        e.endLine();
      }
      e.level(-1);

      e.beginLine();
      e.code += '}';
      break;
    }
    case 'ExpressionStatement': {
      emit(e, node.expr, node);
      e.code += ';';
      break;
    }
  }
}
