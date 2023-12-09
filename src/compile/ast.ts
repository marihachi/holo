export type Loc = {
  line: number;
  column: number;
};

export type SyntaxNode = Unit | FunctionDecl | Expression | Statement;

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

export type Expression = NumberLiteral | Reference | Binary | Unary | If | Switch | Block | Call;

export function isExpr(node: SyntaxNode): node is Expression {
  switch (node.kind) {
    case 'NumberLiteral':
    case 'Reference':
    case 'Binary':
    case 'Unary':
    case 'If':
    case 'Switch':
    case 'Block':
    case 'Call':
      return true;
  }
  return false;
}

export class NumberLiteral {
  kind = 'NumberLiteral' as const;
  constructor(
    public value: number,
    public loc: Loc,
  ) {}
}

export class Reference {
  kind = 'Reference' as const;
  constructor(
    public name: string,
    public loc: Loc,
  ) {}
}

export type BinaryMode = 'add' | 'sub' | 'mul' | 'div' | 'rem' | 'and' | 'or' | 'eq' | 'neq' | 'lt' | 'lte' | 'gt' | 'gte'
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

export type UnaryMode = 'not' | 'compl' | 'plus' | 'minus';

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
    public elseExpr: Expression | undefined,
    public loc: Loc,
  ) {}
}

export class Switch {
  kind = 'Switch' as const;
  constructor(
    public expr: Expression,
    public arms: { cond: Expression, thenBlock: Block }[],
    public defaultBlock: Block | undefined,
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

export class Call {
  kind = 'Call' as const;
  constructor(
    public expr: Expression,
    public args: Expression[],
    public loc: Loc,
  ) {}
}

export type Statement = VariableDecl | Break | Continue | Return | Assign | While | ExpressionStatement;

export class Break {
  kind = 'Break' as const;
  constructor(
    public loc: Loc,
  ) {}
}

export class Continue {
  kind = 'Continue' as const;
  constructor(
    public loc: Loc,
  ) {}
}

export class Return {
  kind = 'Return' as const;
  constructor(
    public expr: Expression | undefined,
    public loc: Loc,
  ) {}
}

export type AssignMode = 'simple' | 'add' | 'sub' | 'mul' | 'div' | 'rem' | 'shl' | 'shr' | 'bitand' | 'bitor' | 'xor';

export class Assign {
  kind = 'Assign' as const;
  constructor(
    public mode: AssignMode,
    public left: Expression,
    public right: Expression,
    public loc: Loc,
  ) {}
}

export class While {
  kind = 'While' as const;
  constructor(
    public mode: 'while' | 'do-while',
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
