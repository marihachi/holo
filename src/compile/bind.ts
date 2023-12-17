import { SyntaxNode, Unit } from './ast.js';
import { FunctionSymbol, SemanticSymbol, VariableSymbol } from './symbol.js';

export class Symbols {
  table: Map<SyntaxNode, SemanticSymbol> = new Map();

  setSymbol(node: SyntaxNode, symbol: SemanticSymbol): void {
    this.table.set(node, symbol);
  }

  getSymbol(node: SyntaxNode): SemanticSymbol | undefined {
    return this.table.get(node);
  }
}

class Environment {
  constructor(
    public parent: Environment | undefined,
  ) {}
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
      symbols.setSymbol(node, new FunctionSymbol(node.name));
      const funcEnv = new Environment(env);
      for (const child of node.body) {
        bindNode(child, funcEnv, symbols);
      }
      break;
    }
    case 'VariableDecl': {
      symbols.setSymbol(node, new VariableSymbol(node.name));
      if (node.expr != null) {
        bindNode(node.expr, env, symbols);
      }
      break;
    }
  }
}
