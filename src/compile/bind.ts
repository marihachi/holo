import { SyntaxNode, Unit } from './ast.js';
import { FunctionSymbol, SemanticSymbol, VariableSymbol } from './symbol.js';

export class Symbols {
  table: Map<SyntaxNode, SemanticSymbol> = new Map();

  set(node: SyntaxNode, symbol: SemanticSymbol): void {
    this.table.set(node, symbol);
  }

  get(node: SyntaxNode): SemanticSymbol | undefined {
    return this.table.get(node);
  }
}

class Environment {
  table: Map<string, SemanticSymbol> = new Map();

  constructor(
    public parent: Environment | undefined,
  ) {}

  set(name: string, symbol: SemanticSymbol): void {
    this.table.set(name, symbol);
  }

  get(name: string): SemanticSymbol | undefined {
    const symbol = this.table.get(name);
    if (symbol != null) {
      return symbol;
    }
    return this.parent?.get(name);
  }
}

export function bind(ast: Unit): Symbols {
  const symbols = new Symbols();

  const grobalEnv = new Environment(undefined);
  for (const child of ast.decls) {
    bindNode(child, grobalEnv, symbols);
  }

  return symbols;
}

function bindNode(node: SyntaxNode, env: Environment, symbols: Symbols): void {
  switch (node.kind) {
    case 'FunctionDecl': {
      const symbol = new FunctionSymbol(node.name);
      symbols.set(node, symbol);
      env.set(node.name, symbol);
      const funcEnv = new Environment(env);
      for (const child of node.body) {
        bindNode(child, funcEnv, symbols);
      }
      break;
    }
    case 'VariableDecl': {
      const symbol = new VariableSymbol(node.name);
      symbols.set(node, symbol);
      env.set(node.name, symbol);
      if (node.expr != null) {
        bindNode(node.expr, env, symbols);
      }
      break;
    }
    case 'NumberLiteral': {
      break;
    }
    case 'Reference': {
      const symbol = env.get(node.name);
      if (symbol == null) {
        throw new Error(`unknown identifier: ${node.name}`);
      }
      // 参照ノードにも宣言と同じシンボルを使ってもよいか？
      symbols.set(node, symbol);
      break;
    }
    case 'Binary': {
      break;
    }
    case 'Unary': {
      break;
    }
    case 'If': {
      bindNode(node.cond, env, symbols);
      bindNode(node.thenExpr, env, symbols);
      if (node.elseExpr != null) {
        bindNode(node.elseExpr, env, symbols);
      }
      break;
    }
    case 'Block': {
      const blockEnv = new Environment(env);
      for (const child of node.body) {
        bindNode(child, blockEnv, symbols);
      }
      break;
    }
    case 'Call': {
      break;
    }
    case 'Break': {
      break;
    }
    case 'Continue': {
      break;
    }
    case 'Return': {
      break;
    }
    case 'Assign': {
      break;
    }
    case 'While': {
      break;
    }
    case 'Switch': {
      break;
    }
    case 'ExpressionStatement': {
      break;
    }
  }
}
