import {
  Assign,
  Binary,
  Block,
  Expression,
  If,
  Reference,
  Return,
  Statement,
  SyntaxNode,
  Unit,
  VariableDecl,
  isExpression,
  isStatement,
} from './ast.js';

export function lowering(node: Unit): Unit {
  desugar(node.decls);
  return node;
}

// function desugarExpr(unit: Unit): Unit {
//   function needLowering(node: Expression): boolean {
//     let found = false;
//     visit(node, (ctx) => {
//       switch (ctx.node.kind) {
//         case 'If':
//         case 'Switch':
//         case 'Block': {
//           found = true;
//           break;
//         }
//       }
//       return !found;
//     });
//     return found;
//   }
//
//   return visit(unit, (ctx) => {
//     console.log(ctx.node.kind);
//     switch (ctx.node.kind) {
//       case 'VariableDecl':
//       case 'Return':
//       case 'ExpressionStatement': {
//         if (ctx.node.expr != null && needLowering(ctx.node.expr)) {
//           console.log('lowering');
//         }
//         break;
//       }
//       case 'Assign': {
//         if (ctx.node.target != null && needLowering(ctx.node.target)) {
//           console.log('lowering');
//         }
//         if (ctx.node.expr != null && needLowering(ctx.node.expr)) {
//           console.log('lowering');
//         }
//         break;
//       }
//       case 'While': {
//         if (ctx.node.expr != null && needLowering(ctx.node.expr)) {
//           console.log('lowering');
//         }
//         for (let i = 0; i < ctx.node.body.length; i++) {
//           const step = ctx.node.body[i];
//           if (isExpression(step) && needLowering(step)) {
//             console.log('lowering');
//           }
//         }
//         break;
//       }
//     }
//     return true;
//   });
// }

function desugarSwitch(ctx: BlockVisitorContext): void {
  // TODO
}

function desugarIf(ctx: BlockVisitorContext): void {
  const node = ctx.node;

  switch (node.kind) {
    case "VariableDecl": {
      const expr = node.expr;

      if (expr?.kind == 'If') {
        node.expr = undefined;

        transformLastExpr(expr, (expr, _, release) => {
          if (expr.kind == "If") return release(expr);
          return new Assign("simple", new Reference(node.name, expr.loc), expr, expr.loc);
        }, node => !(node.kind == "VariableDecl"));

        ctx.insert(expr);
      }

      return;
    }

    case "Return": {
      const expr = node.expr;

      if (expr?.kind == 'If') {
        transformLastExpr(expr, (expr, _, release) => {
          if (expr.kind == "If") return release(expr);
          return new Return(expr, expr.loc);
        });

        ctx.replace(expr);
      }

      return;
    }
  }

  return;
}

function desugarBlock(ctx: BlockVisitorContext): void {
  // TODO
}

function desugar<N extends SyntaxNode>(body: N[]): N[] {
  return visitBlock(body, (ctx) => {
    desugarIf(ctx);
    desugarBlock(ctx);
    desugarSwitch(ctx);
  });
}

/**
 * ASTをトラバースする
 *
 * ハンドラのctx.nodeから必要に応じてノードを上書きできる。
 * ハンドラがfalseを返すと内側ノードのトラバースをスキップできる。
 */
function visit<N extends SyntaxNode>(node: N, f: (ctx: { node: SyntaxNode }) => boolean): N {
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

interface BlockVisitorContext<N extends SyntaxNode = SyntaxNode> {
  node: N;
  block: N[];
  index: number;
  /** 次の位置に挿入する、挿入したノードも処理される */
  insert(node: N): void;
  /** 次の位置に挿入する、挿入したノードは処理されない */
  insertSkip(node: N): void;
  /** 現在のノードを置換 */
  replace(...nodes: N[]): void;
  /** 次に来るノードをn回分読み飛ばす */
  skip(n: number): void;
}

/**
 * ノードを追加したりするのに特化させたvisitor
 */
function visitBlock<N extends SyntaxNode>(body: N[], fn: (ctx: BlockVisitorContext) => void): N[] {
  for (let i = 0; i < body.length; i++) {
    const node = body[i];

    const ctx: BlockVisitorContext<N> = {
      node: node,
      block: body,
      index: i,
      insert(node) {
        body.splice(i + 1, 0, node);
      },
      insertSkip(node) {
        body.splice(i += 1, 0, node);
      },
      replace(...nodes) {
        body.splice(i, 1, ...nodes);
      },
      skip(n) {
        i += n;
      }
    }

    fn(ctx);

    switch (node.kind) {
      case "Block": {
        visitBlock(node.body, fn);
        break;
      }

      case "FunctionDecl": {
        visitBlock(node.body, fn);
        break;
      }

      case "VariableDecl": {
        if (node.expr?.kind == "Block") {
          visitBlock(node.expr.body, fn);
        }

        break;
      }

      case "If": {
        if (node.cond.kind == "Block") {
          visitBlock(node.cond.body, fn);
        }

        if (node.thenExpr.kind == "Block") {
          visitBlock(node.thenExpr.body, fn);
        }

        if (node.elseExpr?.kind == "Block") {
          visitBlock(node.elseExpr.body, fn);
        }
        break;
      }
    }
  }

  return body;
}

type ReleaseFn = (node: Expression | Statement) => Expression | Statement;

type TransformState = {
  nestLevel: number;
  pos?: {
    block: (Expression | Statement)[];
    index: number;
    node?: Expression | Return;
  }
};

/*
 * ブロックの最後に評価される式を置換するやつ
 */
function transformLastExpr(
  node: SyntaxNode,
  fn: (expr: Expression, parent: SyntaxNode, release: ReleaseFn) => Expression | Statement,
  filter?: (node: SyntaxNode) => boolean,
  state: TransformState = { nestLevel: 0 }
) {
  const releaseFn = (): ReleaseFn => (node) => (transformLastExpr(node, fn), node);

  visit(node, (ctx) => {
    if (ctx.node.kind == "FunctionDecl" || ctx.node.kind == "Block") {
      const body = ctx.node.body;
      const pos = state.pos ??= { block: body, index: 0 };

      const level = state.nestLevel++;

      loop: for (let i = 0; i < body.length; i++) {
        const child = body[i];

        pos.block = body;

        if (i == body.length - 1) {
          if (isExpression(child)) {
            pos.index = i;
            pos.node = child;
            break loop;
          }
        }

        switch (child.kind) {
          case "Continue":
          case "Break": {
            if (i > 0) {
              const prevNode = body[i - 1];
              if (isExpression(prevNode)) {
                pos.index = i - 1;
                pos.node = prevNode;
                break loop;
              }
            }
            break;
          }

          case "Return": {
            if (child.expr) {
              pos.index = i;
              pos.node = child;

              break loop;
            }
          }

          default: {
            transformLastExpr(child, fn, filter, state);
          }
        }
      }

      if (level <= 0) {
        if (pos.node) {
          switch (pos.node.kind) {
            case "Return": {
              if (pos.node.expr) {
                let res = fn(pos.node.expr, ctx.node, releaseFn());
                if (isExpression(res)) {
                  pos.node.expr = res;
                  res = pos.node;
                }
              
                pos.block[pos.index] = res;
              }

              break;
            }

            default: {
              let res = fn(pos.node, ctx.node, releaseFn());

              pos.block[pos.index] = res;
              break;
            }
          }
        }

        state.pos = undefined;
      }

      state.nestLevel--;

      return false;
    }

    return filter?.(ctx.node) ?? true;
  });
}
