import {
  AssignNode,
  ExpressionNode,
  FunctionDeclNode,
  ReferenceNode,
  ReturnNode,
  StatementNode,
  SyntaxNode,
  UnitNode,
  isContainerNode,
  isExpressionNode,
} from './syntax-node.js';

export function lowering(node: UnitNode): UnitNode {
  desugarFuncReturnExpr(node);

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

/**
 * 関数直下にある式をreturn文に置き換える。  
 * ただし、式が関数の最後のステップにない場合は文法エラーを生成する。
*/
function desugarFuncReturnExpr(node: UnitNode) {
  function desugarFunc(decl: FunctionDeclNode) {
    for (let i = 0; i < decl.body.length; i++) {
      const step = decl.body[i];
      if (isExpressionNode(step)) {
        if (i == decl.body.length - 1) {
          decl.body[i] = new ReturnNode(step, step.loc);
        } else {
          throw new Error('statement expected.');
        }
      }
    }
  }

  for (const decl of node.decls) {
    if (decl.kind == 'FunctionDeclNode') {
      desugarFunc(decl);
    }
  }
}

function desugarSwitch(ctx: NodeVisitorContext<ContainerContext>): void {
  const cCtx = ctx.subCtx!;
  // TODO
}

function desugarIf(ctx: NodeVisitorContext<ContainerContext>): void {
  const cCtx = ctx.subCtx!;
  const node = cCtx.getNode();
  switch (node.kind) {
    case 'VariableDeclNode': {
      const expr = node.expr;
      if (expr != null && expr.kind == 'IfNode') {
        // 初期化式のない変数宣言にする
        node.expr = undefined;
        replaceLastExprInContainer(expr, (e, _parent, release) => {
          if (e.kind == 'IfNode') return release(e);
          return new AssignNode('simple', new ReferenceNode(node.name, e.loc), e, e.loc);
        }, node => (node.kind != 'VariableDeclNode'));
        // 変数宣言の次のステップにif式を生成
        cCtx.insertNext(expr);
      }
      break;
    }
    case 'ReturnNode': {
      const expr = node.expr;
      if (expr != null && expr.kind == 'IfNode') {
        replaceLastExprInContainer(expr, (e, _parent, release) => {
          if (e.kind == 'IfNode') return release(e);
          return new ReturnNode(e, e.loc);
        });
        // return文を置き換える
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

export class NodeVisitorContext<T, U extends SyntaxNode = SyntaxNode> {
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
export function visitNode<T, U extends SyntaxNode>(
  node: U,
  handler: (ctx: NodeVisitorContext<T>) => boolean,
): U {
  const ctx = new NodeVisitorContext<T, U>(node);
  if (handler(ctx)) {
    const vNode = ctx.getNode();
    switch (vNode.kind) {
      case 'UnitNode': {
        for (let i = 0; i < vNode.decls.length; i++) {
          vNode.decls[i] = visitNode(vNode.decls[i], handler);
        }
        break;
      }
      case 'FunctionDeclNode': {
        for (let i = 0; i < vNode.body.length; i++) {
          vNode.body[i] = visitNode(vNode.body[i], handler);
        }
        break;
      }
      case 'VariableDeclNode': {
        if (vNode.expr != null) {
          vNode.expr = visitNode(vNode.expr, handler);
        }
        break;
      }
      case 'NumberLiteralNode': {
        break;
      }
      case 'ReferenceNode': {
        break;
      }
      case 'BinaryNode': {
        vNode.left = visitNode(vNode.left, handler);
        vNode.right = visitNode(vNode.right, handler);
        break;
      }
      case 'UnaryNode': {
        vNode.expr = visitNode(vNode.expr, handler);
        break;
      }
      case 'IfNode': {
        vNode.cond = visitNode(vNode.cond, handler);
        vNode.thenExpr = visitNode(vNode.thenExpr, handler);
        if (vNode.elseExpr != null) {
          vNode.elseExpr = visitNode(vNode.elseExpr, handler);
        }
        break;
      }
      case 'BlockNode': {
        for (let i = 0; i < vNode.body.length; i++) {
          vNode.body[i] = visitNode(vNode.body[i], handler);
        }
        break;
      }
      case 'CallNode': {
        vNode.expr = visitNode(vNode.expr, handler);
        for (let i = 0; i < vNode.args.length; i++) {
          vNode.args[i] = visitNode(vNode.args[i], handler);
        }
        break;
      }
      case 'BreakNode': {
        break;
      }
      case 'ContinueNode': {
        break;
      }
      case 'ReturnNode': {
        if (vNode.expr != null) {
          vNode.expr = visitNode(vNode.expr, handler);
        }
        break;
      }
      case 'AssignNode': {
        vNode.target = visitNode(vNode.target, handler);
        vNode.expr = visitNode(vNode.expr, handler);
        break;
      }
      case 'WhileNode': {
        vNode.expr = visitNode(vNode.expr, handler);
        for (let i = 0; i < vNode.body.length; i++) {
          vNode.body[i] = visitNode(vNode.body[i], handler);
        }
        break;
      }
      case 'SwitchNode': {
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
      case 'ExpressionStatementNode': {
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
      case 'FunctionDeclNode':
      case 'WhileNode':
      case 'BlockNode': {
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
    expr: ExpressionNode,
    parent: SyntaxNode,
    release: ((node: ExpressionNode | StatementNode) => ExpressionNode | StatementNode)
  ) => ExpressionNode | StatementNode;

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
    if (isContainerNode(vNode)) {
      const pos = findLastExprInContainer(vNode.body, filter);

      if (!pos) return filter?.(vNode) ?? true;

      switch (pos.node.kind) {
        case "ReturnNode": {
          if (pos.node.expr) {
            let res = fn(pos.node.expr, vNode, (n) => {
              replaceLastExprInContainer(n, fn, filter);
              return n;
            });

            // 帰ってきたノードが式であるならreturnでラップする
            if (isExpressionNode(res)) {
              pos.node.expr = res;
              res = pos.node;
            }

            pos.container[pos.index] = res;
          }
          break;
        }
        default: {
          let res = fn(pos.node, vNode, (n) => {
            replaceLastExprInContainer(n, fn, filter);
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
   * 式の位置
   */
  pos?: Readonly<{
    container: (ExpressionNode | StatementNode)[];
    index: number;
    node: ExpressionNode | ReturnNode;
  }>;
};

/**
 * コンテナの最後に評価される式を探す
 */
function findLastExprInContainer(body: (StatementNode | ExpressionNode)[], filter?: (node: SyntaxNode) => boolean, ctx: FindLastExprContext = { }) {
  loop: for (let i = 0; i < body.length; i++) {
    const child = body[i];

    // ループの終端であれば
    if (i == body.length - 1) {
      if (isExpressionNode(child)) {
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
      case 'ContinueNode':
      case 'BreakNode': {
        // 後ろにノードがあればその位置を記録
        if (i > 0) {
          const prevNode = body[i - 1];
          if (isExpressionNode(prevNode)) {
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

      case 'ReturnNode': {
        if (child.expr != null) {
          ctx.pos = {
            container: body,
            index: i,
            node: child
          };
          break loop;
        } else if (i > 0) {
          const prevNode = body[i - 1];
          if (isExpressionNode(prevNode)) {
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

          if (isContainerNode(node)) {
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
