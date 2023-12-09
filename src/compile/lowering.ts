import { SyntaxNode, Unit } from './ast.js';

export function lowering(node: Unit): Unit {
  node = transformExprStack(node);
  return node;
}

/**
 * 式をスタック操作に変換
*/
function transformExprStack(unit: Unit): Unit {
  function transform(node: SyntaxNode): any {
    switch (node.kind) {
      case 'Unit': {
        for (let i = 0; i < node.decls.length; i++) {
          node.decls[i] = transform(node.decls[i]);
        }
        break;
      }
      case 'FunctionDecl': {
        for (let i = 0; i < node.body.length; i++) {
          node.body[i] = transform(node.body[i]);
        }
        break;
      }
      case 'VariableDecl': {
        if (node.body != null) {
          node.body = transform(node.body);
        }
        break;
      }
      case 'NumberLiteral': {
        break;
      }
      case 'Reference': {
        break;
      }
      case 'Binary': {
        node.left = transform(node.left);
        node.right = transform(node.right);
        break;
      }
      case 'Unary': {
        node.expr = transform(node.expr);
        break;
      }
      case 'If': {
        node.cond = transform(node.cond);
        node.thenExpr = transform(node.thenExpr);
        if (node.elseExpr != null) {
          node.elseExpr = transform(node.elseExpr);
        }
        break;
      }
      case 'Block': {
        for (let i = 0; i < node.body.length; i++) {
          node.body[i] = transform(node.body[i]);
        }
        break;
      }
      case 'Call': {
        node.expr = transform(node.expr);
        for (let i = 0; i < node.args.length; i++) {
          node.args[i] = transform(node.args[i]);
        }
        break;
      }
      case 'Break': {
        break;
      }
      case 'Continue': {
        break;
      }
      case 'Return': {
        if (node.expr != null) {
          node.expr = transform(node.expr);
        }
        break;
      }
      case 'Assign': {
        node.left = transform(node.left);
        node.right = transform(node.right);
        break;
      }
      case 'While': {
        node.cond = transform(node.cond);
        for (let i = 0; i < node.body.length; i++) {
          node.body[i] = transform(node.body[i]);
        }
        break;
      }
      case 'Switch': {
        node.expr = transform(node.expr);
        for (let i = 0; i < node.arms.length; i++) {
          node.arms[i].cond = transform(node.arms[i].cond);
          node.arms[i].thenBlock = transform(node.arms[i].thenBlock);
        }
        if (node.defaultBlock != null) {
          node.defaultBlock = transform(node.defaultBlock);
        }
        break;
      }
      case 'ExpressionStatement': {
        node.expr = transform(node.expr);
        break;
      }
    }
    return node;
  }

  return transform(unit);
}

function loweringSwitch(node: Unit): Unit {
  // TODO
  return node;
}
