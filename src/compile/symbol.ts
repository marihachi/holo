import { FunctionDecl, VariableDecl } from './ast.js';
import { Type } from './type.js';

export type SemanticSymbol =
  | FunctionSymbol
  | VariableSymbol
  | ParameterSymbol
  | TypeSymbol;

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

export class TypeSymbol {
  kind = 'TypeSymbol' as const;
  constructor(
    public name: string,
    public suffixes: ({ kind: 'array', dimension: number } | { kind: 'pointer' })[],
    public type: Type,
  ) {}
}
