import { SyntaxNode, UnitNode, isExpressionNode } from './syntax-node.js';
import { Symbols } from './bind.js';

export class Emitter {
  code: string;
  indentLevel: number;

  constructor() {
    this.code = '';
    this.indentLevel = 0;
  }

  indent() {
    this.code += '  '.repeat(this.indentLevel);
  }

  newLine() {
    this.code += '\n';
  }

  level(levelOffset: number) {
    this.indentLevel += levelOffset;
  }
}

export function generate(node: UnitNode, symbols: Symbols) {
  const e = new Emitter();
  emit(e, node);
  return e.code;
}

function emit(e: Emitter, node: SyntaxNode, parent?: SyntaxNode) {
  switch (node.kind) {
    case 'UnitNode': {
      for (const decl of node.decls) {
        e.indent();
        emit(e, decl, node);
        e.newLine();
      }
      break;
    }
    case 'FunctionDeclNode': {
      e.code += node.typeRef?.name ?? '';
      e.code += ' ';
      e.code += node.name;
      e.code += '(';
      for (let i = 0; i < node.parameters.length; i++) {
        if (i > 0) e.code += ', ';
        e.code += node.parameters[i].typeRef?.name ?? '';
        e.code += ' ';
        e.code += node.parameters[i].name;
      }
      e.code += ')';
      e.code += ' {';
      e.newLine();

      e.level(1);
      for (const step of node.body) {
        e.indent();
        emit(e, step, node);
        e.newLine();
      }
      e.level(-1);

      e.indent();
      e.code += '}';
      break;
    }
    case 'VariableDeclNode': {
      if (node.typeRef != null) {
        e.code += node.typeRef.name;
        e.code += ' ';
        for (const suffix of node.typeRef.suffixes) {
          e.code += '*';
        }
      }
      e.code += node.name;
      if (node.expr != null) {
        e.newLine();
        e.level(1);
        e.indent();
        e.code += '= ';
        emit(e, node.expr, node);
        e.code += ';';
        e.level(-1);
      } else {
        e.code += ';';
      }
      break;
    }
    case 'NumberLiteralNode': {
      e.code += node.value;
      break;
    }
    case 'ReferenceNode': {
      e.code += node.name;
      break;
    }
    case 'BinaryNode': {
      // TODO
      switch (node.mode) {
        case 'add': {
          emit(e, node.left, node);
          e.code += ' + ';
          emit(e, node.right, node);
          break;
        }
      }
      break;
    }
    case 'UnaryNode': {
      // TODO
      break;
    }
    case 'IfNode': {
      // TODO
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
    case 'BlockNode': {
      // TODO
      e.code += '{';
      e.newLine();
      e.level(1);
      for (const child of node.body) {
        e.indent();
        emit(e, child, node);
        e.newLine();
      }
      e.level(-1);
      e.indent();
      e.code += '}';
      break;
    }
    case 'CallNode': {
      // TODO
      break;
    }
    case 'BreakNode': {
      e.code += 'break';
      e.code += ';';
      break;
    }
    case 'ContinueNode': {
      e.code += 'continue';
      e.code += ';';
      break;
    }
    case 'ReturnNode': {
      e.code += 'return';
      if (node.expr != null) {
        e.code += ' ';
        emit(e, node.expr, node);
      }
      e.code += ';';
      break;
    }
    case 'AssignNode': {
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
      e.code += ';';
      break;
    }
    case 'WhileNode': {
      e.code += 'while ';
      e.code += '(';
      emit(e, node.expr, node);
      e.code += ')';
      e.code += ' {';
      e.newLine();

      e.level(1);
      for (const step of node.body) {
        e.indent();
        emit(e, step, node);
        e.newLine();
      }
      e.level(-1);

      e.indent();
      e.code += '}';
      break;
    }
    case 'SwitchNode': {
      e.code += 'switch ';
      e.code += '(';
      emit(e, node.expr, node);
      e.code += ')';
      e.code += ' {';
      e.newLine();

      e.level(1);
      for (const arm of node.arms) {
        e.indent();
        e.code += 'case ';
        emit(e, arm.cond, node);
        e.code += ': {';
        e.newLine();
        let blockResult;
        for (const step of arm.thenBlock.body) {
          if (!isExpressionNode(step)) {
            e.indent();
            emit(e, step, arm.thenBlock);
            e.newLine();
          }
          blockResult = step;
        }
        // TODO: support Assign
        if (parent != null && blockResult != null && parent.kind == 'VariableDeclNode' && isExpressionNode(blockResult)) {
          e.indent();
          e.code += `${parent.name} = `;
          emit(e, blockResult, arm.thenBlock);
          e.code += ';';
          e.newLine();
        }
        e.indent();
        e.code += '}';
        e.newLine();
      }
      e.level(-1);

      e.indent();
      e.code += '}';
      break;
    }
    case 'ExpressionStatementNode': {
      emit(e, node.expr, node);
      e.code += ';';
      break;
    }
  }
}
