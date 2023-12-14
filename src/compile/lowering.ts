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
  // 全てのコンテナを見る
  visitContainer(node, ctx => {
    const cCtx = ctx.subCtx!;
    while (!cCtx.endOfStream()) {
      desugarSwitch(ctx);
      desugarIf(ctx);
      desugarBlock(ctx);
      cCtx.seek(1);
    }
    return true;
  });

  return node;
}

function desugarSwitch(ctx: NodeVisitorContext<ContainerContext>): void {
  const cCtx = ctx.subCtx!;
  // TODO
}

function desugarIf(ctx: NodeVisitorContext<ContainerContext>): void {
  const cCtx = ctx.subCtx!;
  const node = cCtx.getNode();
  switch (node.kind) {
    case 'VariableDecl': {
      const expr = node.expr;
      if (expr != null && expr.kind == 'If') {
        node.expr = undefined;
        replaceLastExprInContainer(expr, (e, _parent, release) => {
          if (e.kind == 'If') return release(e);
          return new Assign('simple', new Reference(node.name, e.loc), e, e.loc);
        }, node => (node.kind != 'VariableDecl'));
        cCtx.insertNext(expr);
      }
      break;
    }
    case 'Return': {
      const expr = node.expr;
      if (expr != null && expr.kind == 'If') {
        replaceLastExprInContainer(expr, (e, _parent, release) => {
          if (e.kind == 'If') return release(e);
          return new Return(e, e.loc);
        });
        cCtx.replace(expr);
      }
      break;
    }
  }
}

function desugarBlock(ctx: NodeVisitorContext<ContainerContext>): void {
  const cCtx = ctx.subCtx!;
  // TODO
}

class NodeVisitorContext<T, U extends SyntaxNode = SyntaxNode> {
  private _node: U;

  subCtx?: T;

  constructor(node: U) {
    this._node = node;
  }

  getNode(): U {
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
  handler: (ctx: NodeVisitorContext<T>) => boolean,
): U {
  const ctx = new NodeVisitorContext<T, U>(node);
  if (handler(ctx)) {
    const vNode = ctx.getNode();
    switch (vNode.kind) {
      case 'Unit': {
        for (let i = 0; i < vNode.decls.length; i++) {
          vNode.decls[i] = visitNode(vNode.decls[i], handler);
        }
        break;
      }
      case 'FunctionDecl': {
        for (let i = 0; i < vNode.body.length; i++) {
          vNode.body[i] = visitNode(vNode.body[i], handler);
        }
        break;
      }
      case 'VariableDecl': {
        if (vNode.expr != null) {
          vNode.expr = visitNode(vNode.expr, handler);
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
        vNode.left = visitNode(vNode.left, handler);
        vNode.right = visitNode(vNode.right, handler);
        break;
      }
      case 'Unary': {
        vNode.expr = visitNode(vNode.expr, handler);
        break;
      }
      case 'If': {
        vNode.cond = visitNode(vNode.cond, handler);
        vNode.thenExpr = visitNode(vNode.thenExpr, handler);
        if (vNode.elseExpr != null) {
          vNode.elseExpr = visitNode(vNode.elseExpr, handler);
        }
        break;
      }
      case 'Block': {
        for (let i = 0; i < vNode.body.length; i++) {
          vNode.body[i] = visitNode(vNode.body[i], handler);
        }
        break;
      }
      case 'Call': {
        vNode.expr = visitNode(vNode.expr, handler);
        for (let i = 0; i < vNode.args.length; i++) {
          vNode.args[i] = visitNode(vNode.args[i], handler);
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
        if (vNode.expr != null) {
          vNode.expr = visitNode(vNode.expr, handler);
        }
        break;
      }
      case 'Assign': {
        vNode.target = visitNode(vNode.target, handler);
        vNode.expr = visitNode(vNode.expr, handler);
        break;
      }
      case 'While': {
        vNode.expr = visitNode(vNode.expr, handler);
        for (let i = 0; i < vNode.body.length; i++) {
          vNode.body[i] = visitNode(vNode.body[i], handler);
        }
        break;
      }
      case 'Switch': {
        vNode.expr = visitNode(vNode.expr, handler);
        for (let i = 0; i < vNode.arms.length; i++) {
          vNode.arms[i].cond = visitNode(vNode.arms[i].cond, handler);
          vNode.arms[i].thenBlock = visitNode(vNode.arms[i].thenBlock, handler);
        }
        if (vNode.defaultBlock != null) {
          vNode.defaultBlock = visitNode(vNode.defaultBlock, handler);
        }
        break;
      }
      case 'ExpressionStatement': {
        vNode.expr = visitNode(vNode.expr, handler);
        break;
      }
    }
  }

  return ctx.getNode();
}

class ContainerContext<T extends SyntaxNode = SyntaxNode> {
  constructor(
    public container: T[],
    public index: number
  ) {}

  endOfStream(): boolean {
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
function visitContainer<T extends SyntaxNode = SyntaxNode>(
  node: T,
  handler: (ctx: NodeVisitorContext<ContainerContext>) => boolean,
): void {
  visitNode<ContainerContext, T>(node, ctx => {
    const vNode = ctx.getNode();
    switch (vNode.kind) {
      case 'FunctionDecl':
      case 'While':
      case 'Block': {
        ctx.subCtx = new ContainerContext(vNode.body, 0);
        return handler(ctx);
      }
    }
    return true;
  });
}

type LastExprReplacer =
  /**
   * @param parent - `expr`の親ノード
   * @param release - 生成されたノードを再帰的に置換するための関数
   */
  (
    expr: Expression,
    parent: SyntaxNode,
    release: ((node: Expression | Statement) => Expression | Statement)
  ) => Expression | Statement;

/**
 * コンテナの最後に評価される式を置換する
 */
function replaceLastExprInContainer(
  node: SyntaxNode,
  fn: LastExprReplacer,
  filter?: (node: SyntaxNode) => boolean
): void {
  visitNode(node, (ctx) => {
    const vNode = ctx.getNode();

    // コンテナであれば置換を開始する
    if (vNode.kind == "FunctionDecl" || vNode.kind == "Block") {
      const pos = findLastExprInContainer(vNode.body, filter);

      if (!pos) return filter?.(vNode) ?? true;

      switch (pos.node.kind) {
        case "Return": {
          if (pos.node.expr) {
            let res = fn(pos.node.expr, vNode, (n) => {
              replaceLastExprInContainer(n, fn);
              return n;
            });

            // 帰ってきたノードが式であるならreturnでラップする
            if (isExpression(res)) {
              pos.node.expr = res;
              res = pos.node;
            }

            pos.container[pos.index] = res;
          }
          break;
        }
        default: {
          let res = fn(pos.node, vNode, (n) => {
            replaceLastExprInContainer(n, fn);
            return n;
          });

          pos.container[pos.index] = res;
          break;
        }
      }

      return false;
    }

    // 置換されると都合の悪いものはフィルタリングしてもらう
    return filter?.(vNode) ?? true;
  });
}

type FindLastExprContext = {
  /**
   * コンテナのネストレベル
   * 最後に評価される式の追跡で使用する
   */
  nestLevel: number;

  /**
   * 式の位置
   */
  pos?: Readonly<{
    container: (Expression | Statement)[];
    index: number;
    node: Expression | Return;
  }>;
};

/**
 * コンテナの最後に評価される式を探す
 */
function findLastExprInContainer(body: (Statement | Expression)[], filter?: (node: SyntaxNode) => boolean, ctx: FindLastExprContext = { nestLevel: 0 }) {
  loop: for (let i = 0; i < body.length; i++) {
    const child = body[i];

    // ループの終端であれば
    if (i == body.length - 1) {
      if (isExpression(child)) {
        ctx.pos = {
          container: body,
          index: i,
          node: child
        };
        break loop;
      }
    }

    // ブロックから抜け出す処理がある場合、その時に評価される式の位置を記録
    // そうでなければそのノードの内容から再帰的に記録していく
    switch (child.kind) {
      case 'Continue':
      case 'Break': {
        // 後ろにノードがあればその位置を記録
        if (i > 0) {
          const prevNode = body[i - 1];
          if (isExpression(prevNode)) {
            ctx.pos = {
              container: body,
              index: i - 1,
              node: prevNode,
            };
            break loop;
          }
        }
        break;
      }

      case 'Return': {
        if (child.expr != null) {
          ctx.pos = {
            container: body,
            index: i,
            node: child
          };
          break loop;
        } else if (i > 0) {
          const prevNode = body[i - 1];
          if (isExpression(prevNode)) {
            ctx.pos = {
              container: body,
              index: i - 1,
              node: prevNode,
            };
            break loop;
          }
        }
        break;
      }

      default: {
        // 子コンテナの内容も探査する
        visitNode(child, vCtx => {
          const node = vCtx.getNode();

          if (node.kind == "Block" || node.kind == "FunctionDecl" || node.kind == "While") {
            findLastExprInContainer(node.body, filter, ctx);
          }

          return filter?.(node) ?? true;
        });
        break;
      }
    }
  }

  return ctx.pos;
}
