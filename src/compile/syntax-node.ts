export type SyntaxNode = UnitNode | FunctionDeclNode | FuncParameterNode | TypeRefNode | StatementNode | ExpressionNode;
export type StatementNode = VariableDeclNode | BreakNode | ContinueNode | ReturnNode | AssignNode | WhileNode | ExpressionStatementNode;
export type ExpressionNode = NumberLiteralNode | ReferenceNode | BinaryNode | UnaryNode | IfNode | SwitchNode | BlockNode | CallNode;
export type ContainerNode = BlockNode | FunctionDeclNode | WhileNode;

export function isStatement(node: SyntaxNode): node is StatementNode {
  switch (node.kind) {
    case 'VariableDeclNode':
    case 'BreakNode':
    case 'ContinueNode':
    case 'ReturnNode':
    case 'AssignNode':
    case 'WhileNode':
    case 'ExpressionStatementNode':
      return true;
  }
  return false;
}

export function isExpressionNode(node: SyntaxNode): node is ExpressionNode {
  switch (node.kind) {
    case 'NumberLiteralNode':
    case 'ReferenceNode':
    case 'BinaryNode':
    case 'UnaryNode':
    case 'IfNode':
    case 'SwitchNode':
    case 'BlockNode':
    case 'CallNode':
      return true;
  }
  return false;
}

export function isContainerNode(node: SyntaxNode): node is ContainerNode {
  switch (node.kind) {
    case "BlockNode":
    case "FunctionDeclNode":
    case "WhileNode":
      return true;
  }

  return false;
}

export type Loc = {
  line: number;
  column: number;
};

// SyntaxNode

export class UnitNode {
  kind = 'UnitNode' as const;
  constructor(
    public decls: (FunctionDeclNode | VariableDeclNode)[],
    public loc: Loc,
  ) {}
}

export class FuncParameterNode {
  kind = 'FuncParameterNode' as const;
  constructor(
    public name: string,
    public typeRef: TypeRefNode | undefined,
    public loc: Loc,
  ) {}
}

export class FunctionDeclNode {
  kind = 'FunctionDeclNode' as const;
  constructor(
    public name: string,
    public parameters: FuncParameterNode[],
    public typeRef: TypeRefNode | undefined,
    public body: (ExpressionNode | StatementNode)[],
    public loc: Loc,
  ) {}
}

export class TypeRefNode {
  kind = 'TypeRefNode' as const;
  constructor(
    public name: string,
    public suffixes: ({ kind: 'array', dimensions: { size: number | undefined }[] } | { kind: 'pointer' })[],
    public loc: Loc,
  ) {}
}

// Statement

export class VariableDeclNode {
  kind = 'VariableDeclNode' as const;
  constructor(
    public name: string,
    public typeRef: TypeRefNode | undefined,
    public expr: ExpressionNode | undefined,
    public loc: Loc,
  ) {}
}

export class BreakNode {
  kind = 'BreakNode' as const;
  constructor(
    public loc: Loc,
  ) {}
}

export class ContinueNode {
  kind = 'ContinueNode' as const;
  constructor(
    public loc: Loc,
  ) {}
}

export class ReturnNode {
  kind = 'ReturnNode' as const;
  constructor(
    public expr: ExpressionNode | undefined,
    public loc: Loc,
  ) {}
}

export type AssignMode = 'simple' | 'add' | 'sub' | 'mul' | 'div' | 'rem' | 'shl' | 'shr' | 'bitand' | 'bitor' | 'xor';

export class AssignNode {
  kind = 'AssignNode' as const;
  constructor(
    public mode: AssignMode,
    public target: ExpressionNode,
    public expr: ExpressionNode,
    public loc: Loc,
  ) {}
}

export class WhileNode {
  kind = 'WhileNode' as const;
  constructor(
    public mode: 'while' | 'do-while',
    public expr: ExpressionNode,
    public body: (ExpressionNode | StatementNode)[],
    public loc: Loc,
  ) {}
}

export class ExpressionStatementNode {
  kind = 'ExpressionStatementNode' as const;
  constructor(
    public expr: ExpressionNode,
    public loc: Loc,
  ) {}
}

// Expression

export class NumberLiteralNode {
  kind = 'NumberLiteralNode' as const;
  constructor(
    public value: number,
    public loc: Loc,
  ) {}
}

export class ReferenceNode {
  kind = 'ReferenceNode' as const;
  constructor(
    public name: string,
    public loc: Loc,
  ) {}
}

export type BinaryMode = 'add' | 'sub' | 'mul' | 'div' | 'rem' | 'and' | 'or' | 'eq' | 'neq' | 'lt' | 'lte' | 'gt' | 'gte'
  | 'shl' | 'shr' | 'bitand' | 'bitor' | 'xor';

export class BinaryNode {
  kind = 'BinaryNode' as const;
  constructor(
    public mode: BinaryMode,
    public left: ExpressionNode,
    public right: ExpressionNode,
    public loc: Loc,
  ) {}
}

export type UnaryMode = 'not' | 'compl' | 'plus' | 'minus';

export class UnaryNode {
  kind = 'UnaryNode' as const;
  constructor(
    public mode: UnaryMode,
    public expr: ExpressionNode,
    public loc: Loc,
  ) {}
}

export class IfNode {
  kind = 'IfNode' as const;
  constructor(
    public cond: ExpressionNode,
    public thenExpr: ExpressionNode,
    public elseExpr: ExpressionNode | undefined,
    public loc: Loc,
  ) {}
}

export class SwitchNode {
  kind = 'SwitchNode' as const;
  constructor(
    public expr: ExpressionNode,
    public arms: { cond: ExpressionNode, thenBlock: BlockNode }[],
    public defaultBlock: BlockNode | undefined,
    public loc: Loc,
  ) {}
}

export class BlockNode {
  kind = 'BlockNode' as const;
  constructor(
    public body: (ExpressionNode | StatementNode)[],
    //public result: Expression | undefined,
    public loc: Loc,
  ) {}
}

export class CallNode {
  kind = 'CallNode' as const;
  constructor(
    public expr: ExpressionNode,
    public args: ExpressionNode[],
    public loc: Loc,
  ) {}
}
