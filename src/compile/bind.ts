import { SyntaxNode, UnitNode } from './syntax-node.js';
import { HoloFunction, SemanticNode, Variable } from './semantic-node.js';

export class Symbols {
  table: Map<SyntaxNode, SemanticNode> = new Map();

  set(node: SyntaxNode, symbol: SemanticNode): void {
    this.table.set(node, symbol);
  }

  get(node: SyntaxNode): SemanticNode | undefined {
    return this.table.get(node);
  }
}

class Environment {
  table: Map<string, SemanticNode> = new Map();

  constructor(
    public parent: Environment | undefined,
  ) {}

  set(name: string, symbol: SemanticNode): void {
    this.table.set(name, symbol);
  }

  get(name: string): SemanticNode | undefined {
    const symbol = this.table.get(name);
    if (symbol != null) {
      return symbol;
    }
    return this.parent?.get(name);
  }
}

export function bind(ast: UnitNode, symbols: Symbols): void {
  const grobalEnv = new Environment(undefined);
  for (const child of ast.decls) {
    bindNode(child, grobalEnv, symbols);
  }
}

function bindNode(node: SyntaxNode, env: Environment, symbols: Symbols): void {
  switch (node.kind) {
    case 'FunctionDeclNode': {
      const symbol = new HoloFunction(node.name, node, undefined);
      symbols.set(node, symbol);
      env.set(node.name, symbol);

      const funcEnv = new Environment(env);
      for (const child of node.body) {
        bindNode(child, funcEnv, symbols);
      }
      break;
    }
    case 'VariableDeclNode': {
      const symbol = new Variable(node.name, node, undefined);
      symbols.set(node, symbol);
      env.set(node.name, symbol);

      if (node.expr != null) {
        bindNode(node.expr, env, symbols);
      }
      break;
    }
    case 'NumberLiteralNode': {
      break;
    }
    case 'ReferenceNode': {
      const symbol = env.get(node.name);
      if (symbol == null) {
        throw new Error(`unknown identifier: ${node.name}`);
      }
      // 参照ノードにも宣言と同じシンボルを使ってもよいか？
      symbols.set(node, symbol);
      break;
    }
    case 'BinaryNode': {
      bindNode(node.left, env, symbols);
      bindNode(node.right, env, symbols);
      break;
    }
    case 'UnaryNode': {
      bindNode(node.expr, env, symbols);
      break;
    }
    case 'IfNode': {
      bindNode(node.cond, env, symbols);
      bindNode(node.thenExpr, env, symbols);
      if (node.elseExpr != null) {
        bindNode(node.elseExpr, env, symbols);
      }
      break;
    }
    case 'BlockNode': {
      const blockEnv = new Environment(env);
      for (const child of node.body) {
        bindNode(child, blockEnv, symbols);
      }
      break;
    }
    case 'CallNode': {
      bindNode(node.expr, env, symbols);
      for (const child of node.args) {
        bindNode(child, env, symbols);
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
      if (node.expr != null) {
        bindNode(node.expr, env, symbols);
      }
      break;
    }
    case 'AssignNode': {
      bindNode(node.target, env, symbols);
      bindNode(node.expr, env, symbols);
      break;
    }
    case 'WhileNode': {
      bindNode(node.expr, env, symbols);
      const blockEnv = new Environment(env);
      for (const child of node.body) {
        bindNode(child, blockEnv, symbols);
      }
      break;
    }
    case 'SwitchNode': {
      bindNode(node.expr, env, symbols);
      for (const arm of node.arms) {
        bindNode(arm.cond, env, symbols);
        bindNode(arm.thenBlock, env, symbols);
      }
      if (node.defaultBlock != null) {
        bindNode(node.defaultBlock, env, symbols);
      }
      break;
    }
    case 'ExpressionStatementNode': {
      bindNode(node.expr, env, symbols);
      break;
    }
  }
}
