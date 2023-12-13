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
  desugar(node.decls);
  return node;
}

function desugar<T extends SyntaxNode>(body: T[]) {
  // コンテナ毎に見ていく
  visitContainer(body, (ctx) => {
    desugarSwitch(ctx);
    desugarIf(ctx);
    desugarBlock(ctx);
  });
}

function desugarSwitch(ctx: ContainerVisitorContext): void {
  // TODO
}

function desugarIf(ctx: ContainerVisitorContext): void {
  const node = ctx.getNode();

  switch (node.kind) {
    case 'VariableDecl': {
      const expr = node.expr;
      if (expr != null && expr.kind == 'If') {
        node.expr = undefined;
        transformLastExpr(expr, (e, _, release) => {
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
        transformLastExpr(expr, (e, _, release) => {
          if (e.kind == 'If') return release(e);
          return new Return(e, e.loc);
        });
        ctx.replace(expr);
      }
      break;
    }
  }
}

function desugarBlock(ctx: ContainerVisitorContext): void {
  // TODO
}

class NodeVisitorContext<T extends SyntaxNode = SyntaxNode> {
  private _node: T;

  constructor(node: T) {
    this._node = node;
  }

  get node(): T {
    return this._node;
  }

  /** 現在のノードを置換 */
  replace(node: T): void {
    this._node = node;
  }
}

/**
 * ASTを訪問する
 *
 * NodeVisitorContext.nodeから必要に応じてノードを上書きできる。
 * ハンドラがfalseを返すと内側ノードのトラバースをスキップできる。
 */
function visitNode<T extends SyntaxNode>(
  node: T,
  handler: (ctx: NodeVisitorContext) => boolean,
): T {
  const ctx = new NodeVisitorContext(node);
  if (handler(ctx)) {
    switch (ctx.node.kind) {
      case 'Unit': {
        for (let i = 0; i < ctx.node.decls.length; i++) {
          ctx.node.decls[i] = visitNode(ctx.node.decls[i], handler);
        }
        break;
      }
      case 'FunctionDecl': {
        for (let i = 0; i < ctx.node.body.length; i++) {
          ctx.node.body[i] = visitNode(ctx.node.body[i], handler);
        }
        break;
      }
      case 'VariableDecl': {
        if (ctx.node.expr != null) {
          ctx.node.expr = visitNode(ctx.node.expr, handler);
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
        ctx.node.left = visitNode(ctx.node.left, handler);
        ctx.node.right = visitNode(ctx.node.right, handler);
        break;
      }
      case 'Unary': {
        ctx.node.expr = visitNode(ctx.node.expr, handler);
        break;
      }
      case 'If': {
        ctx.node.cond = visitNode(ctx.node.cond, handler);
        ctx.node.thenExpr = visitNode(ctx.node.thenExpr, handler);
        if (ctx.node.elseExpr != null) {
          ctx.node.elseExpr = visitNode(ctx.node.elseExpr, handler);
        }
        break;
      }
      case 'Block': {
        for (let i = 0; i < ctx.node.body.length; i++) {
          ctx.node.body[i] = visitNode(ctx.node.body[i], handler);
        }
        break;
      }
      case 'Call': {
        ctx.node.expr = visitNode(ctx.node.expr, handler);
        for (let i = 0; i < ctx.node.args.length; i++) {
          ctx.node.args[i] = visitNode(ctx.node.args[i], handler);
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
          ctx.node.expr = visitNode(ctx.node.expr, handler);
        }
        break;
      }
      case 'Assign': {
        ctx.node.target = visitNode(ctx.node.target, handler);
        ctx.node.expr = visitNode(ctx.node.expr, handler);
        break;
      }
      case 'While': {
        ctx.node.expr = visitNode(ctx.node.expr, handler);
        for (let i = 0; i < ctx.node.body.length; i++) {
          ctx.node.body[i] = visitNode(ctx.node.body[i], handler);
        }
        break;
      }
      case 'Switch': {
        ctx.node.expr = visitNode(ctx.node.expr, handler);
        for (let i = 0; i < ctx.node.arms.length; i++) {
          ctx.node.arms[i].cond = visitNode(ctx.node.arms[i].cond, handler);
          ctx.node.arms[i].thenBlock = visitNode(ctx.node.arms[i].thenBlock, handler);
        }
        if (ctx.node.defaultBlock != null) {
          ctx.node.defaultBlock = visitNode(ctx.node.defaultBlock, handler);
        }
        break;
      }
      case 'ExpressionStatement': {
        ctx.node.expr = visitNode(ctx.node.expr, handler);
        break;
      }
    }
  }

  return ctx.node;
}

class ContainerVisitorContext<T extends SyntaxNode = SyntaxNode> {
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

/**
 * コンテナにノードを追加・削除できるvisitor
*/
function visitContainer<T extends SyntaxNode>(
  container: T[],
  handler: (ctx: ContainerVisitorContext) => void,
): void {
  const ctx = new ContainerVisitorContext(container, 0);
  while (!ctx.endOfStream()) {
    handler(ctx);
    const node = ctx.getNode();
    switch (node.kind) {
      case 'FunctionDecl':
      case 'Block': {
        visitContainer(node.body, handler);
        break;
      }
      case 'VariableDecl': {
        if (node.expr?.kind == 'Block') {
          visitContainer(node.expr.body, handler);
        }
        break;
      }
      case 'If': {
        if (node.cond.kind == 'Block') {
          visitContainer(node.cond.body, handler);
        }
        if (node.thenExpr.kind == 'Block') {
          visitContainer(node.thenExpr.body, handler);
        }
        if (node.elseExpr?.kind == 'Block') {
          visitContainer(node.elseExpr.body, handler);
        }
        break;
      }
    }
    ctx.seek(1);
  }
}

type ReleaseFn = (node: Expression | Statement) => Expression | Statement;

type TransformState = {
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
  state: TransformState = { nestLevel: 0 },
): void {
  const releaseFn = (): ReleaseFn => (node) => (transformLastExpr(node, fn), node);
  visitNode(node, (ctx) => {
    if (ctx.node.kind == 'FunctionDecl' || ctx.node.kind == 'Block') {
      const body = ctx.node.body;
      state.pos ??= { container: body, index: 0 };
      const level = state.nestLevel++;
      loop: for (let i = 0; i < body.length; i++) {
        const child = body[i];
        state.pos.container = body;
        if (i == body.length - 1) {
          if (isExpression(child)) {
            state.pos.index = i;
            state.pos.node = child;
            break loop;
          }
        }
        switch (child.kind) {
          case 'Continue':
          case 'Break': {
            if (i > 0) {
              const prevNode = body[i - 1];
              if (isExpression(prevNode)) {
                state.pos.index = i - 1;
                state.pos.node = prevNode;
                break loop;
              }
            }
            break;
          }
          case 'Return': {
            if (child.expr) {
              state.pos.index = i;
              state.pos.node = child;
              break loop;
            }
            break;
          }
          default: {
            transformLastExpr(child, fn, filter, state);
            break;
          }
        }
      }
      if (level <= 0) {
        if (state.pos.node) {
          switch (state.pos.node.kind) {
            case 'Return': {
              if (state.pos.node.expr) {
                let res = fn(state.pos.node.expr, ctx.node, releaseFn());
                if (isExpression(res)) {
                  state.pos.node.expr = res;
                  res = state.pos.node;
                }
                state.pos.container[state.pos.index] = res;
              }
              break;
            }
            default: {
              let res = fn(state.pos.node, ctx.node, releaseFn());
              state.pos.container[state.pos.index] = res;
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
