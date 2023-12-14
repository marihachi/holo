export type SemanticSymbol =
  | FunctionSymbol
  | VariableSymbol
  | ParameterSymbol
  | OperatorSymbol
  | BlockSymbol;

export class FunctionSymbol {
  kind = 'FunctionSymbol' as const;
  constructor(
    public name: string,
  ) {}
}

export class VariableSymbol {
  kind = 'VariableSymbol' as const;
  constructor(
    public name: string,
  ) {}
}

export class ParameterSymbol {
  kind = 'ParameterSymbol' as const;
  constructor(
    public name: string,
  ) {}
}

export class OperatorSymbol {
  kind = 'OperatorSymbol' as const;
}

export class BlockSymbol {
  kind = 'BlockSymbol' as const;
}
