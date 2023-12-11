import { Expression, SyntaxNode, Unit, isExpression, isStatement } from './ast.js';

export function lowering(node: Unit): Unit {
  node = desugarExpr(node);
  return node;
}

function desugarExpr(unit: Unit): Unit {
  function needLowering(node: Expression): boolean {
    let found = false;
    visit(node, ctx => {
      switch (ctx.node.kind) {
        case 'If':
        case 'Switch':
        case 'Block': {
          found = true;
          break;
        }
      }
      return !found;
    });
    return found;
  }

  return visit(unit, ctx => {
    console.log(ctx.node.kind);
    switch (ctx.node.kind) {
      case 'VariableDecl':
      case 'Return':
      case 'ExpressionStatement': {
        if (ctx.node.expr != null && needLowering(ctx.node.expr)) {
          console.log('lowering');
        }
        break;
      }
      case 'Assign': {
        if (ctx.node.target != null && needLowering(ctx.node.target)) {
          console.log('lowering');
        }
        if (ctx.node.expr != null && needLowering(ctx.node.expr)) {
          console.log('lowering');
        }
        break;
      }
      case 'While': {
        if (ctx.node.expr != null && needLowering(ctx.node.expr)) {
          console.log('lowering');
        }
        for (let i = 0; i < ctx.node.body.length; i++) {
          const step = ctx.node.body[i];
          if (isExpression(step) && needLowering(step)) {
            console.log('lowering');
          }
        }
        break;
      }
    }
    return true;
  });
}

/**
 * ASTをトラバースする
 *
 * ハンドラのctx.nodeから必要に応じてノードを上書きできる。
 * ハンドラがfalseを返すと内側ノードのトラバースをスキップできる。
*/
function visit(node: SyntaxNode, f: (ctx: { node: SyntaxNode }) => boolean): any {
  const ctx = { node };
  if (f(ctx)) {
    switch (ctx.node.kind) {
      case 'Unit': {
        for (let i = 0; i < ctx.node.decls.length; i++) {
          ctx.node.decls[i] = visit(ctx.node.decls[i], f);
        }
        break;
      }
      case 'FunctionDecl': {
        for (let i = 0; i < ctx.node.body.length; i++) {
          ctx.node.body[i] = visit(ctx.node.body[i], f);
        }
        break;
      }
      case 'VariableDecl': {
        if (ctx.node.expr != null) {
          ctx.node.expr = visit(ctx.node.expr, f);
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
        ctx.node.left = visit(ctx.node.left, f);
        ctx.node.right = visit(ctx.node.right, f);
        break;
      }
      case 'Unary': {
        ctx.node.expr = visit(ctx.node.expr, f);
        break;
      }
      case 'If': {
        ctx.node.cond = visit(ctx.node.cond, f);
        ctx.node.thenExpr = visit(ctx.node.thenExpr, f);
        if (ctx.node.elseExpr != null) {
          ctx.node.elseExpr = visit(ctx.node.elseExpr, f);
        }
        break;
      }
      case 'Block': {
        for (let i = 0; i < ctx.node.body.length; i++) {
          ctx.node.body[i] = visit(ctx.node.body[i], f);
        }
        break;
      }
      case 'Call': {
        ctx.node.expr = visit(ctx.node.expr, f);
        for (let i = 0; i < ctx.node.args.length; i++) {
          ctx.node.args[i] = visit(ctx.node.args[i], f);
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
        if (ctx.node.expr != null) {
          ctx.node.expr = visit(ctx.node.expr, f);
        }
        break;
      }
      case 'Assign': {
        ctx.node.target = visit(ctx.node.target, f);
        ctx.node.expr = visit(ctx.node.expr, f);
        break;
      }
      case 'While': {
        ctx.node.expr = visit(ctx.node.expr, f);
        for (let i = 0; i < ctx.node.body.length; i++) {
          ctx.node.body[i] = visit(ctx.node.body[i], f);
        }
        break;
      }
      case 'Switch': {
        ctx.node.expr = visit(ctx.node.expr, f);
        for (let i = 0; i < ctx.node.arms.length; i++) {
          ctx.node.arms[i].cond = visit(ctx.node.arms[i].cond, f);
          ctx.node.arms[i].thenBlock = visit(ctx.node.arms[i].thenBlock, f);
        }
        if (ctx.node.defaultBlock != null) {
          ctx.node.defaultBlock = visit(ctx.node.defaultBlock, f);
        }
        break;
      }
      case 'ExpressionStatement': {
        ctx.node.expr = visit(ctx.node.expr, f);
        break;
      }
    }
  }

  return ctx.node;
}
