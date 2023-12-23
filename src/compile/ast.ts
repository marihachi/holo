export type SyntaxNode = Unit | FunctionDecl | FuncParameter | TypeRef | Statement | Expression;
export type Statement = VariableDecl | Break | Continue | Return | Assign | While | ExpressionStatement;
export type Expression = NumberLiteral | Reference | Binary | Unary | If | Switch | Block | Call;
export type ContainerNode = Block | FunctionDecl | While;

export function isStatement(node: SyntaxNode): node is Statement {
  switch (node.kind) {
    case 'VariableDecl':
    case 'Break':
    case 'Continue':
    case 'Return':
    case 'Assign':
    case 'While':
    case 'ExpressionStatement':
      return true;
  }
  return false;
}

export function isExpression(node: SyntaxNode): node is Expression {
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

export function isContainerNode(node: SyntaxNode): node is ContainerNode {
  switch (node.kind) {
    case "Block":
    case "FunctionDecl":
    case "While":
      return true;
  }

  return false;
}

export type Loc = {
  line: number;
  column: number;
};

// SyntaxNode

export class Unit {
  kind = 'Unit' as const;
  constructor(
    public decls: (FunctionDecl | VariableDecl)[],
    public loc: Loc,
  ) {}
}

export class FuncParameter {
  kind = 'FuncParameter' as const;
  constructor(
    public name: string,
    public typeRef: TypeRef | undefined,
    public loc: Loc,
  ) {}
}

export class FunctionDecl {
  kind = 'FunctionDecl' as const;
  constructor(
    public name: string,
    public parameters: FuncParameter[],
    public typeRef: TypeRef | undefined,
    public body: (Expression | Statement)[],
    public loc: Loc,
  ) {}
}

export class TypeRef {
  kind = 'TypeRef' as const;
  constructor(
    public name: string,
    public suffixes: ({ kind: 'array', dimensions: { size: number | undefined }[] } | { kind: 'pointer' })[],
    public loc: Loc,
  ) {}
}

// Statement

export class VariableDecl {
  kind = 'VariableDecl' as const;
  constructor(
    public name: string,
    public typeRef: TypeRef | undefined,
    public expr: Expression | undefined,
    public loc: Loc,
  ) {}
}

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
    public target: Expression,
    public expr: Expression,
    public loc: Loc,
  ) {}
}

export class While {
  kind = 'While' as const;
  constructor(
    public mode: 'while' | 'do-while',
    public expr: Expression,
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

// Expression

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
