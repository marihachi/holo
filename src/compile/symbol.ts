import { FunctionDecl, VariableDecl } from './ast.js';

export type SemanticSymbol =
  | FunctionSymbol
  | VariableSymbol
  | ParameterSymbol;

export class FunctionSymbol {
  kind = 'FunctionSymbol' as const;
  constructor(
    public name: string,
    public node: FunctionDecl,
  ) {}
}

export class VariableSymbol {
  kind = 'VariableSymbol' as const;
  constructor(
    public name: string,
    public node: VariableDecl,
  ) {}
}

export class ParameterSymbol {
  kind = 'ParameterSymbol' as const;
  constructor(
    public name: string,
  ) {}
}
