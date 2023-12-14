import { SyntaxNode, Unit } from './ast.js';
import { SemanticSymbol } from './symbol.js';

class BindContext {
  constructor(
    public ast: Unit,
    public symbols: Map<SyntaxNode, SemanticSymbol>,
  ) {}
}

export function bind(ast: Unit): Map<SyntaxNode, SemanticSymbol> {
  // TODO
  const ctx = new BindContext(ast, new Map());

  return ctx.symbols;
}
