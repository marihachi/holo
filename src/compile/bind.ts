import { SyntaxNode, UnitNode } from './syntax-node.js';
import { HoloFunction, SemanticNode, Variable } from './semantic-node.js';

// 宣言ノードに対してsemantic nodeを生成します。
// 参照ノードの名前を解決しsemantic nodeと関連付けます。

export class SemanticTable {
  table: Map<SyntaxNode, SemanticNode> = new Map();

  set(node: SyntaxNode, symbol: SemanticNode): void {
    this.table.set(node, symbol);
  }

  get(node: SyntaxNode): SemanticNode | undefined {
    return this.table.get(node);
  }
}

class NameTable {
  table: Map<string, SemanticNode> = new Map();

  constructor(
    public parent: NameTable | undefined,
  ) {}

  set(name: string, symbol: SemanticNode): void {
    this.table.set(name, symbol);
  }

  get(name: string): SemanticNode | undefined {
    const symbol = this.table.get(name);
    if (symbol != null) {
      return symbol;
    }
    return this.parent?.get(name);
  }
}

export function bind(ast: UnitNode, semanticTable: SemanticTable): void {
  const nameTable = new NameTable(undefined);
  for (const child of ast.decls) {
    visitNode(child, nameTable, semanticTable);
  }
}

function visitNode(node: SyntaxNode, nameTable: NameTable, semanticTable: SemanticTable): void {
  switch (node.kind) {
    case 'FunctionDeclNode': {
      const symbol = new HoloFunction(node.name, node, undefined);
      semanticTable.set(node, symbol);
      nameTable.set(node.name, symbol);

      const innerNameTable = new NameTable(nameTable);
      for (const child of node.body) {
        visitNode(child, innerNameTable, semanticTable);
      }
      break;
    }
    case 'VariableDeclNode': {
      const symbol = new Variable(node.name, node, undefined);
      semanticTable.set(node, symbol);
      nameTable.set(node.name, symbol);

      if (node.expr != null) {
        visitNode(node.expr, nameTable, semanticTable);
      }
      break;
    }
    case 'NumberLiteralNode': {
      break;
    }
    case 'ReferenceNode': {
      const semantic = nameTable.get(node.name);
      if (semantic == null) {
        throw new Error(`unknown identifier: ${node.name}`);
      }
      // 参照ノードにも宣言と同じシンボルを使ってもよいか？
      semanticTable.set(node, semantic);
      break;
    }
    case 'BinaryNode': {
      visitNode(node.left, nameTable, semanticTable);
      visitNode(node.right, nameTable, semanticTable);
      break;
    }
    case 'UnaryNode': {
      visitNode(node.expr, nameTable, semanticTable);
      break;
    }
    case 'IfNode': {
      visitNode(node.cond, nameTable, semanticTable);
      visitNode(node.thenExpr, nameTable, semanticTable);
      if (node.elseExpr != null) {
        visitNode(node.elseExpr, nameTable, semanticTable);
      }
      break;
    }
    case 'BlockNode': {
      const innerNameTable = new NameTable(nameTable);
      for (const child of node.body) {
        visitNode(child, innerNameTable, semanticTable);
      }
      break;
    }
    case 'CallNode': {
      visitNode(node.expr, nameTable, semanticTable);
      for (const child of node.args) {
        visitNode(child, nameTable, semanticTable);
      }
      break;
    }
    case 'BreakNode': {
      break;
    }
    case 'ContinueNode': {
      break;
    }
    case 'ReturnNode': {
      if (node.expr != null) {
        visitNode(node.expr, nameTable, semanticTable);
      }
      break;
    }
    case 'AssignNode': {
      visitNode(node.target, nameTable, semanticTable);
      visitNode(node.expr, nameTable, semanticTable);
      break;
    }
    case 'WhileNode': {
      visitNode(node.expr, nameTable, semanticTable);
      const innerNameTable = new NameTable(nameTable);
      for (const child of node.body) {
        visitNode(child, innerNameTable, semanticTable);
      }
      break;
    }
    case 'SwitchNode': {
      visitNode(node.expr, nameTable, semanticTable);
      for (const arm of node.arms) {
        visitNode(arm.cond, nameTable, semanticTable);
        visitNode(arm.thenBlock, nameTable, semanticTable);
      }
      if (node.defaultBlock != null) {
        visitNode(node.defaultBlock, nameTable, semanticTable);
      }
      break;
    }
    case 'ExpressionStatementNode': {
      visitNode(node.expr, nameTable, semanticTable);
      break;
    }
  }
}
