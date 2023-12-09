export type Symbol =
  | FunctionSymbol
  | VariableSymbol
  | ParameterSymbol
  | OperatorSymbol
  | BlockSymbol;

export class FunctionSymbol {
  kind = 'FunctionSymbol' as const;
}

export class VariableSymbol {
  kind = 'VariableSymbol' as const;
}

export class ParameterSymbol {
  kind = 'ParameterSymbol' as const;
}

export class OperatorSymbol {
  kind = 'OperatorSymbol' as const;
}

export class BlockSymbol {
  kind = 'BlockSymbol' as const;
}
