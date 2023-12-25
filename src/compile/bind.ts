import { SyntaxNode, UnitNode } from './syntax-node.js';
import { HoloFunction, SemanticNode, TypeSymbol, Variable } from './semantic-node.js';
import { PrimitiveType } from './type.js';

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

  const typeTable = new NameTable(undefined);
  declarePrimitiveTypes(typeTable);

  for (const child of ast.decls) {
    visitNode(child, nameTable, typeTable, semanticTable);
  }
}

function declarePrimitiveTypes(typeTable: NameTable) {
  typeTable.set('int', new TypeSymbol('int', new PrimitiveType('int')));
}

function visitNode(node: SyntaxNode, nameTable: NameTable, typeTable: NameTable, semanticTable: SemanticTable): void {
  switch (node.kind) {
    case 'FunctionDeclNode': {
      const symbol = new HoloFunction(node.name, node, undefined);
      semanticTable.set(node, symbol);
      nameTable.set(node.name, symbol);

      const innerNameTable = new NameTable(nameTable);
      const innerTypeTable = new NameTable(typeTable);
      for (const child of node.body) {
        visitNode(child, innerNameTable, innerTypeTable, semanticTable);
      }
      break;
    }
    case 'VariableDeclNode': {
      const symbol = new Variable(node.name, node, undefined);
      semanticTable.set(node, symbol);
      nameTable.set(node.name, symbol);

      if (node.expr != null) {
        visitNode(node.expr, nameTable, typeTable, semanticTable);
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
      visitNode(node.left, nameTable, typeTable, semanticTable);
      visitNode(node.right, nameTable, typeTable, semanticTable);
      break;
    }
    case 'UnaryNode': {
      visitNode(node.expr, nameTable, typeTable, semanticTable);
      break;
    }
    case 'IfNode': {
      visitNode(node.cond, nameTable, typeTable, semanticTable);
      visitNode(node.thenExpr, nameTable, typeTable, semanticTable);
      if (node.elseExpr != null) {
        visitNode(node.elseExpr, nameTable, typeTable, semanticTable);
      }
      break;
    }
    case 'BlockNode': {
      const innerNameTable = new NameTable(nameTable);
      const innerTypeTable = new NameTable(typeTable);
      for (const child of node.body) {
        visitNode(child, innerNameTable, innerTypeTable, semanticTable);
      }
      break;
    }
    case 'CallNode': {
      visitNode(node.expr, nameTable, typeTable, semanticTable);
      for (const child of node.args) {
        visitNode(child, nameTable, typeTable, semanticTable);
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
        visitNode(node.expr, nameTable, typeTable, semanticTable);
      }
      break;
    }
    case 'AssignNode': {
      visitNode(node.target, nameTable, typeTable, semanticTable);
      visitNode(node.expr, nameTable, typeTable, semanticTable);
      break;
    }
    case 'WhileNode': {
      visitNode(node.expr, nameTable, typeTable, semanticTable);
      const innerNameTable = new NameTable(nameTable);
      const innerTypeTable = new NameTable(typeTable);
      for (const child of node.body) {
        visitNode(child, innerNameTable, innerTypeTable, semanticTable);
      }
      break;
    }
    case 'SwitchNode': {
      visitNode(node.expr, nameTable, typeTable, semanticTable);
      for (const arm of node.arms) {
        visitNode(arm.cond, nameTable, typeTable, semanticTable);
        visitNode(arm.thenBlock, nameTable, typeTable, semanticTable);
      }
      if (node.defaultBlock != null) {
        visitNode(node.defaultBlock, nameTable, typeTable, semanticTable);
      }
      break;
    }
    case 'ExpressionStatementNode': {
      visitNode(node.expr, nameTable, typeTable, semanticTable);
      break;
    }
  }
}
