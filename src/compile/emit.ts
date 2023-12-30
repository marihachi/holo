import { UnitSymbol } from './semantic-node.js';
import { SyntaxNode, UnitNode, isExpressionNode } from './syntax-node.js';

export class Emitter {
  private _code: string;
  private _level: number;

  constructor() {
    this._code = '';
    this._level = 0;
  }

  getResult(): string {
    return this._code;
  }

  code(code: string) {
    this._code += code;
  }

  indent() {
    this._code += '  '.repeat(this._level);
  }

  newLine() {
    this._code += '\n';
  }

  enter() {
    this._level += 1;
  }

  leave() {
    this._level -= 1;
  }
}

export function emit(node: UnitNode, unitSymbol: UnitSymbol) {
  const e = new Emitter();
  emitNode(e, node);
  return e.getResult();
}

function emitNode(e: Emitter, node: SyntaxNode, parent?: SyntaxNode) {
  switch (node.kind) {
    case 'UnitNode': {
      for (const decl of node.decls) {
        e.indent();
        emitNode(e, decl, node);
        e.newLine();
      }
      break;
    }
    case 'FunctionDeclNode': {
      e.code(node.typeRef?.name ?? '');
      e.code(' ');
      e.code(node.name);
      e.code('(');
      for (let i = 0; i < node.parameters.length; i++) {
        if (i > 0) e.code(', ');
        e.code(node.parameters[i].typeRef?.name ?? '');
        e.code(' ');
        e.code(node.parameters[i].name);
      }
      e.code(') {');
      e.newLine();

      e.enter();
      for (const step of node.body) {
        e.indent();
        emitNode(e, step, node);
        e.newLine();
      }
      e.leave();

      e.indent();
      e.code('}');
      break;
    }
    case 'VariableDeclNode': {
      if (node.typeRef != null) {
        e.code(node.typeRef.name);
        e.code(' ');
        for (const suffix of node.typeRef.suffixes) {
          e.code('*');
        }
      }
      e.code(node.name);
      if (node.expr != null) {
        e.newLine();

        e.enter();
        e.indent();
        e.code('= ');
        emitNode(e, node.expr, node);
        e.code(';');
        e.leave();

      } else {
        e.code(';');
      }
      break;
    }
    case 'NumberLiteralNode': {
      e.code(node.value.toString());
      break;
    }
    case 'ReferenceNode': {
      e.code(node.name);
      break;
    }
    case 'BinaryNode': {
      // TODO
      switch (node.mode) {
        case 'add': {
          emitNode(e, node.left, node);
          e.code(' + ');
          emitNode(e, node.right, node);
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
      e.code('if (');
      emitNode(e, node.cond, node);
      e.code(') ');
      emitNode(e, node.thenExpr, node);
      if (node.elseExpr) {
        e.code(' else ');
        emitNode(e, node.elseExpr, node);
      }
      break;
    }
    case 'BlockNode': {
      // TODO
      e.code('{');
      e.newLine();

      e.enter();
      for (const child of node.body) {
        e.indent();
        emitNode(e, child, node);
        e.newLine();
      }
      e.leave();

      e.indent();
      e.code('}');
      break;
    }
    case 'CallNode': {
      // TODO
      break;
    }
    case 'BreakNode': {
      e.code('break');
      e.code(';');
      break;
    }
    case 'ContinueNode': {
      e.code('continue');
      e.code(';');
      break;
    }
    case 'ReturnNode': {
      e.code('return');
      if (node.expr != null) {
        e.code(' ');
        emitNode(e, node.expr, node);
      }
      e.code(';');
      break;
    }
    case 'AssignNode': {
      emitNode(e, node.target, node);
      switch (node.mode) {
        case 'simple': { e.code(' = '); break; }
        case 'add': { e.code(' += '); break; }
        case 'sub': { e.code(' -= '); break; }
        case 'mul': { e.code(' *= '); break; }
        case 'div': { e.code(' /= '); break; }
        case 'rem': { e.code(' %= '); break; }
        case 'bitand': { e.code(' &= '); break; }
        case 'bitor': { e.code(' |= '); break; }
        case 'shl': { e.code(' <<= '); break; }
        case 'shr': { e.code(' >>= '); break; }
      }
      emitNode(e, node.expr, node);
      e.code(';');
      break;
    }
    case 'WhileNode': {
      e.code('while ');
      e.code('(');
      emitNode(e, node.expr, node);
      e.code(')');
      e.code(' {');
      e.newLine();

      e.enter();
      for (const step of node.body) {
        e.indent();
        emitNode(e, step, node);
        e.newLine();
      }
      e.leave();

      e.indent();
      e.code('}');
      break;
    }
    case 'SwitchNode': {
      e.code('switch ');
      e.code('(');
      emitNode(e, node.expr, node);
      e.code(')');
      e.code(' {');
      e.newLine();

      e.enter();
      for (const arm of node.arms) {
        e.indent();
        e.code('case ');
        emitNode(e, arm.cond, node);
        e.code(': {');
        e.newLine();
        let blockResult;
        for (const step of arm.thenBlock.body) {
          if (!isExpressionNode(step)) {
            e.indent();
            emitNode(e, step, arm.thenBlock);
            e.newLine();
          }
          blockResult = step;
        }
        // TODO: support Assign
        if (parent != null && blockResult != null && parent.kind == 'VariableDeclNode' && isExpressionNode(blockResult)) {
          e.indent();
          e.code(`${parent.name} = `);
          emitNode(e, blockResult, arm.thenBlock);
          e.code(';');
          e.newLine();
        }
        e.indent();
        e.code('}');
        e.newLine();
      }
      e.leave();

      e.indent();
      e.code('}');
      break;
    }
    case 'ExpressionStatementNode': {
      emitNode(e, node.expr, node);
      e.code(';');
      break;
    }
  }
}
