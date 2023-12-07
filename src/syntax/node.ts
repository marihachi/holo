export type Loc = {
  line: number;
  column: number;
};

export type SyntaxNode = Unit | FunctionDecl | VariableDecl | Expression | Statement;

export class Unit {
  kind = 'Unit' as const;
  constructor(
    public decls: (FunctionDecl | VariableDecl)[],
    public loc: Loc,
  ) {}
}

export class FunctionDecl {
  kind = 'FunctionDecl' as const;
  constructor(
    public name: string,
    public paramNames: string[],
    public body: (Expression | Statement)[],
    public loc: Loc,
  ) {}
}

export class VariableDecl {
  kind = 'VariableDecl' as const;
  constructor(
    public name: string,
    public body: Expression | undefined,
    public loc: Loc,
  ) {}
}

export type Expression = NumberLiteral | Binary | Unary | If | Block;

export class NumberLiteral {
  kind = 'NumberLiteral' as const;
  constructor(
    public value: number,
    public loc: Loc,
  ) {}
}

type BinaryMode = 'add' | 'sub' | 'mul' | 'div' | 'rem' | 'and' | 'or' | 'eq' | 'lt' | 'lte' | 'gt' | 'gte'
  | 'shl' | 'shr' | 'bitand' | 'bitor' | 'xor';

export class Binary {
  kind = 'Binary' as const;
  constructor(
    public mode: BinaryMode,
    public left: Expression,
    public right: Expression,
    public loc: Loc,
  ) {}
}

type UnaryMode = 'not' | 'compl' | 'plus' | 'minus';

export class Unary {
  kind = 'Unary' as const;
  constructor(
    public mode: UnaryMode,
    public expr: Expression,
    public loc: Loc,
  ) {}
}

export class If {
  kind = 'If' as const;
  constructor(
    public cond: Expression,
    public thenExpr: Expression,
    public elseExpr: Expression,
    public loc: Loc,
  ) {}
}

export class Block {
  kind = 'Block' as const;
  constructor(
    public body: (Expression | Statement)[],
    //public result: Expression | undefined,
    public loc: Loc,
  ) {}
}

export type Statement = VariableDecl | Assign | While | ExpressionStatement;

export class Assign {
  kind = 'Assign' as const;
  constructor(
    public mode: 'basic' | 'add' | 'sub' | 'mul' | 'div' | 'rem' | 'shl' | 'shr' | 'bitand' | 'bitor' | 'xor',
    public left: Expression,
    public right: Expression,
    public loc: Loc,
  ) {}
}

export class While {
  kind = 'While' as const;
  constructor(
    public cond: Expression,
    public body: (Expression | Statement)[],
    public loc: Loc,
  ) {}
}

export class DoWhile {
  kind = 'DoWhile' as const;
  constructor(
    public cond: Expression,
    public body: (Expression | Statement)[],
    public loc: Loc,
  ) {}
}

export class ExpressionStatement {
  kind = 'ExpressionStatement' as const;
  constructor(
    public expr: Expression,
    public loc: Loc,
  ) {}
}
