import { FunctionDeclNode, VariableDeclNode } from './syntax-node.js';
import { Type } from './type.js';

export type SemanticNode =
  | FunctionSymbol
  | VariableSymbol
  | FunctionParamSymbol
  | TypeSymbol;

export class FunctionSymbol {
  kind = 'FunctionSymbol' as const;
  constructor(
    public name: string,
    public node: FunctionDeclNode,
    public type: Type | undefined,
  ) {}
}

export class VariableSymbol {
  kind = 'VariableSymbol' as const;
  constructor(
    public name: string,
    public node: VariableDeclNode,
    public type: Type | undefined,
  ) {}
}

export class FunctionParamSymbol {
  kind = 'FunctionParamSymbol' as const;
  constructor(
    public name: string,
    public type: Type | undefined,
  ) {}
}

export class TypeSymbol {
  kind = 'TypeSymbol' as const;
  constructor(
    public name: string,
    public type: Type | undefined,
  ) {}
}
