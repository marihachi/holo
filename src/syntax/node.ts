export type Loc = {
  line: number;
  column: number;
};

export type SyntaxNode = Unit | Expression | Decl;

export class Unit {
  kind = 'Unit' as const;
}

export type Decl = FunctionDecl | VariableDecl;

export class FunctionDecl {
  kind = 'FunctionDecl' as const;
}

export class VariableDecl {
  kind = 'VariableDecl' as const;
}

export type Expression = NumberLiteral;

export class NumberLiteral {
  kind = 'NumberLiteral' as const;
  constructor(
    public value: number,
  ) {}
}
