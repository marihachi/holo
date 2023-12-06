export type Loc = {
  line: number;
  column: number;
};

export type SyntaxNode = Unit | Decl | Expression | Statement;

export class Unit {
  kind = 'Unit' as const;
  constructor(
    public decls: Decl[],
    public loc: Loc,
  ) {}
}

export type Decl = FunctionDecl | VariableDecl;

export class FunctionDecl {
  kind = 'FunctionDecl' as const;
  constructor(
    public loc: Loc,
  ) {}
}

export class VariableDecl {
  kind = 'VariableDecl' as const;
  constructor(
    public loc: Loc,
  ) {}
}

export type Expression = NumberLiteral | Binary | Unary;

export class NumberLiteral {
  kind = 'NumberLiteral' as const;
  constructor(
    public value: number,
    public loc: Loc,
  ) {}
}

export class Binary {
  kind = 'Binary' as const;
  constructor(
    public mode: 'add' | 'sub' | 'mul' | 'div' | 'rem' | 'and' | 'or' | 'shl' | 'shr' | 'bitand' | 'bitor' | 'xor',
    public left: Expression,
    public right: Expression,
    public loc: Loc,
  ) {}
}

export class Unary {
  kind = 'Unary' as const;
  constructor(
    public mode: 'not' | 'compl' | 'plus' | 'minus',
    public expr: Expression,
    public loc: Loc,
  ) {}
}

export class If {
  kind = 'If' as const;
  constructor(
    public mode: 'not' | 'compl' | 'plus' | 'minus',
    public cond: Expression,
    public thenExpr: Expression,
    public elseExpr: Expression,
  ) {}
}

export type Statement = Assign;

export class Assign {
  kind = 'Assign' as const;
  constructor(
    public mode: 'basic' | 'add' | 'sub' | 'mul' | 'div' | 'rem' | 'shl' | 'shr' | 'bitand' | 'bitor' | 'xor',
    public left: Expression,
    public right: Expression,
    public loc: Loc,
  ) {}
}
