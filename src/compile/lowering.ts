import {
  Assign,
  Expression,
  Reference,
  Return,
  Statement,
  SyntaxNode,
  Unit,
  isExpression,
} from './ast.js';

export function lowering(node: Unit): Unit {
  for (const decl of node.decls) {
    switch (decl.kind) {
      case 'FunctionDecl': {
        visitContainer2(decl, decl.body, ctx => {
          console.log(ctx.node.kind);
          return true;
        });
        break;
      }
    }
  }
  desugar(node);
  return node;
}

function visitContainer2<T extends SyntaxNode = SyntaxNode, U extends SyntaxNode = SyntaxNode>(
  node: T,
  body: U[],
  handler: (ctx: NodeVisitorContext<ContainerContext<U>>) => boolean,
): void {
  visitNode(node, new ContainerContext(body, 0), ctx => {
    switch (ctx.node.kind) {
      case 'VariableDecl': {
        return handler(ctx);
      }
    }
    return true;
  });
}

function desugar<T extends SyntaxNode>(node: T) {
  // コンテナ毎に見ていく
  visitContainer(node, (ctx) => {
    desugarSwitch(ctx);
    desugarIf(ctx);
    desugarBlock(ctx);
  });
}

function desugarSwitch(ctx: ContainerContext): void {
  // TODO
}

function desugarIf(ctx: ContainerContext): void {
  const node = ctx.getNode();

  switch (node.kind) {
    case 'VariableDecl': {
      const expr = node.expr;
      if (expr != null && expr.kind == 'If') {
        node.expr = undefined;
        transformLastExpr(expr, (e, _parent, release) => {
          if (e.kind == 'If') return release(e);
          return new Assign('simple', new Reference(node.name, e.loc), e, e.loc);
        }, node => (node.kind != 'VariableDecl'));
        ctx.insertNext(expr);
      }
      break;
    }
    case 'Return': {
      const expr = node.expr;
      if (expr != null && expr.kind == 'If') {
        transformLastExpr(expr, (e, _parent, release) => {
          if (e.kind == 'If') return release(e);
          return new Return(e, e.loc);
        });
        ctx.replace(expr);
      }
      break;
    }
  }
}

function desugarBlock(ctx: ContainerContext): void {
  // TODO
}

class ContainerContext<T extends SyntaxNode = SyntaxNode> {
  constructor(
    public container: T[],
    public index: number
  ) {}

  endOfStream() {
    return (this.index >= this.container.length);
  }

  getNode(): T {
    return this.container[this.index];
  }

  /** 現在のノードを置換 */
  replace(...nodes: T[]): void {
    this.container.splice(this.index, 1, ...nodes);
  }

  /** 次の位置に挿入する */
  insertNext(node: T): void {
    this.container.splice(this.index + 1, 0, node);
  }

  /** 現在位置を変更 */
  seek(offset: number): void {
    this.index += offset;
  }
}

class NodeVisitorContext<T, U extends SyntaxNode = SyntaxNode> {
  private _node: U;

  subCtx: T;

  constructor(node: U, subCtx: T) {
    this.subCtx = subCtx;
    this._node = node;
  }

  get node(): U {
    return this._node;
  }

  /** 現在のノードを置換 */
  replace(node: U): void {
    this._node = node;
  }
}

/**
 * ASTを訪問する
 *
 * ハンドラの戻り値で子ノードを訪問するかを決定できる。
 * subCtxから追加情報をハンドラで参照できる。
 */
function visitNode<T, U extends SyntaxNode>(
  node: U,
  subCtx: T,
  handler: (ctx: NodeVisitorContext<T>) => boolean,
): U {
  const ctx = new NodeVisitorContext(node, subCtx);
  if (handler(ctx)) {
    switch (ctx.node.kind) {
      case 'Unit': {
        for (let i = 0; i < ctx.node.decls.length; i++) {
          ctx.node.decls[i] = visitNode(ctx.node.decls[i], subCtx, handler);
        }
        break;
      }
      case 'FunctionDecl': {
        for (let i = 0; i < ctx.node.body.length; i++) {
          ctx.node.body[i] = visitNode(ctx.node.body[i], subCtx, handler);
        }
        break;
      }
      case 'VariableDecl': {
        if (ctx.node.expr != null) {
          ctx.node.expr = visitNode(ctx.node.expr, subCtx, handler);
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
        ctx.node.left = visitNode(ctx.node.left, subCtx, handler);
        ctx.node.right = visitNode(ctx.node.right, subCtx, handler);
        break;
      }
      case 'Unary': {
        ctx.node.expr = visitNode(ctx.node.expr, subCtx, handler);
        break;
      }
      case 'If': {
        ctx.node.cond = visitNode(ctx.node.cond, subCtx, handler);
        ctx.node.thenExpr = visitNode(ctx.node.thenExpr, subCtx, handler);
        if (ctx.node.elseExpr != null) {
          ctx.node.elseExpr = visitNode(ctx.node.elseExpr, subCtx, handler);
        }
        break;
      }
      case 'Block': {
        for (let i = 0; i < ctx.node.body.length; i++) {
          ctx.node.body[i] = visitNode(ctx.node.body[i], subCtx, handler);
        }
        break;
      }
      case 'Call': {
        ctx.node.expr = visitNode(ctx.node.expr, subCtx, handler);
        for (let i = 0; i < ctx.node.args.length; i++) {
          ctx.node.args[i] = visitNode(ctx.node.args[i], subCtx, handler);
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
          ctx.node.expr = visitNode(ctx.node.expr, subCtx, handler);
        }
        break;
      }
      case 'Assign': {
        ctx.node.target = visitNode(ctx.node.target, subCtx, handler);
        ctx.node.expr = visitNode(ctx.node.expr, subCtx, handler);
        break;
      }
      case 'While': {
        ctx.node.expr = visitNode(ctx.node.expr, subCtx, handler);
        for (let i = 0; i < ctx.node.body.length; i++) {
          ctx.node.body[i] = visitNode(ctx.node.body[i], subCtx, handler);
        }
        break;
      }
      case 'Switch': {
        ctx.node.expr = visitNode(ctx.node.expr, subCtx, handler);
        for (let i = 0; i < ctx.node.arms.length; i++) {
          ctx.node.arms[i].cond = visitNode(ctx.node.arms[i].cond, subCtx, handler);
          ctx.node.arms[i].thenBlock = visitNode(ctx.node.arms[i].thenBlock, subCtx, handler);
        }
        if (ctx.node.defaultBlock != null) {
          ctx.node.defaultBlock = visitNode(ctx.node.defaultBlock, subCtx, handler);
        }
        break;
      }
      case 'ExpressionStatement': {
        ctx.node.expr = visitNode(ctx.node.expr, subCtx, handler);
        break;
      }
    }
  }

  return ctx.node;
}

/**
 * コンテナにノードを追加・削除できるvisitor
*/
function visitContainer<T extends SyntaxNode>(
  containerNode: T,
  handler: (ctx: ContainerContext) => void,
): void {
  switch (containerNode.kind) {
    case 'FunctionDecl':
    case 'While':
    case 'Block': {
      break;
    }
    default: {
      return;
    }
  }
  const ctx = new ContainerContext(containerNode.body, 0);
  while (!ctx.endOfStream()) {
    handler(ctx);
    const node = ctx.getNode();
    switch (node.kind) {
      case 'Block': {
        visitContainer(node, handler);
        break;
      }
      case 'VariableDecl': {
        if (node.expr?.kind == 'Block') {
          visitContainer(node.expr, handler);
        }
        break;
      }
      case 'If': {
        if (node.cond.kind == 'Block') {
          visitContainer(node.cond, handler);
        }
        if (node.thenExpr.kind == 'Block') {
          visitContainer(node.thenExpr, handler);
        }
        if (node.elseExpr?.kind == 'Block') {
          visitContainer(node.elseExpr, handler);
        }
        break;
      }
    }
    ctx.seek(1);
  }
}

type ReleaseFn = (node: Expression | Statement) => Expression | Statement;

type TransformContext = {
  nestLevel: number;
  pos?: {
    container: (Expression | Statement)[];
    index: number;
    node?: Expression | Return;
  }
};

/*
 * コンテナの最後に評価される式を置換するやつ
 */
function transformLastExpr(
  node: SyntaxNode,
  fn: (expr: Expression, parent: SyntaxNode, release: ReleaseFn) => Expression | Statement,
  filter?: (node: SyntaxNode) => boolean,
  tCtx: TransformContext = { nestLevel: 0 },
): void {
  visitNode(node, {}, (ctx) => {
    if (ctx.node.kind == 'FunctionDecl' || ctx.node.kind == 'Block') {
      const body = ctx.node.body;
      tCtx.pos ??= { container: body, index: 0 };
      const level = tCtx.nestLevel++;
      loop: for (let i = 0; i < body.length; i++) {
        const child = body[i];
        tCtx.pos.container = body;
        if (i == body.length - 1) {
          if (isExpression(child)) {
            tCtx.pos.index = i;
            tCtx.pos.node = child;
            break loop;
          }
        }
        switch (child.kind) {
          case 'Continue':
          case 'Break': {
            if (i > 0) {
              const prevNode = body[i - 1];
              if (isExpression(prevNode)) {
                tCtx.pos.index = i - 1;
                tCtx.pos.node = prevNode;
                break loop;
              }
            }
            break;
          }
          case 'Return': {
            if (child.expr != null) {
              tCtx.pos.index = i;
              tCtx.pos.node = child;
              break loop;
            }
            break;
          }
          default: {
            transformLastExpr(child, fn, filter, tCtx);
            break;
          }
        }
      }
      if (level <= 0) {
        if (tCtx.pos.node) {
          switch (tCtx.pos.node.kind) {
            case 'Return': {
              if (tCtx.pos.node.expr != null) {
                let res = fn(tCtx.pos.node.expr, ctx.node, (n) => {
                  transformLastExpr(n, fn);
                  return n;
                });
                if (isExpression(res)) {
                  tCtx.pos.node.expr = res;
                  res = tCtx.pos.node;
                }
                tCtx.pos.container[tCtx.pos.index] = res;
              }
              break;
            }
            default: {
              let res = fn(tCtx.pos.node, ctx.node, (n) => {
                transformLastExpr(n, fn);
                return n;
              });
              tCtx.pos.container[tCtx.pos.index] = res;
              break;
            }
          }
        }
        tCtx.pos = undefined;
      }
      tCtx.nestLevel--;
      return false;
    }
    return filter?.(ctx.node) ?? true;
  });
}
