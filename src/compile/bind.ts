import { SyntaxNode, UnitNode } from './syntax-node.js';
import { BlockSymbol, ContainerSymbol, FunctionParamSymbol, FunctionSymbol, SymbolTable, TypeSymbol, UnitSymbol, VariableSymbol, WhileSymbol } from './semantic-node.js';
import { PrimitiveType } from './type.js';

// 宣言ノードに対してsemantic nodeを生成します。
// 参照ノードの名前を解決しsemantic nodeと関連付けます。

export function bind(ast: UnitNode): UnitSymbol {
  const symbol = new UnitSymbol();
  declarePrimitiveTypes(symbol);
  for (const child of ast.decls) {
    visitNode(child, symbol, symbol.nodeTable);
  }
  return symbol;
}

function declarePrimitiveTypes(symbol: ContainerSymbol) {
  symbol.typeNameTable.set('int', new TypeSymbol('int', new PrimitiveType('int')));
  symbol.typeNameTable.set('uint', new TypeSymbol('uint', new PrimitiveType('uint')));
  symbol.typeNameTable.set('bool', new TypeSymbol('bool', new PrimitiveType('bool')));
}

function visitNode(node: SyntaxNode, parent: ContainerSymbol, nodeTable: SymbolTable<SyntaxNode>): void {
  switch (node.kind) {
    case 'FunctionDeclNode': {
      if (parent.nameTable.get(node.name) != null) {
        throw new Error(`identifier "${node.name}" is already exists`);
      }

      const symbol = new FunctionSymbol(node.name, parent);
      parent.nameTable.set(node.name, symbol);
      nodeTable.set(node, symbol);

      // return type
      if (node.typeRef == null) {
        throw new Error('return type is missing');
      }
      visitNode(node.typeRef, symbol, nodeTable);

      // parameters
      for (const parameter of node.parameters) {
        visitNode(parameter, symbol, nodeTable);
      }

      // body
      for (const child of node.body) {
        visitNode(child, symbol, nodeTable);
      }
      break;
    }
    case 'FuncParameterNode': {
      if (parent.nameTable.get(node.name) != null) {
        throw new Error(`identifier "${node.name}" is already exists`);
      }

      const symbol = new FunctionParamSymbol(node.name);
      parent.nameTable.set(node.name, symbol);
      nodeTable.set(node, symbol);

      // type
      if (node.typeRef == null) {
        throw new Error('parameter type is missing');
      }
      visitNode(node.typeRef, parent, nodeTable);
      break;
    }
    case 'VariableDeclNode': {
      if (parent.nameTable.get(node.name) != null) {
        throw new Error(`identifier "${node.name}" is already exists`);
      }

      const symbol = new VariableSymbol(node.name);
      parent.nameTable.set(node.name, symbol);
      nodeTable.set(node, symbol);

      // type
      if (node.typeRef == null) {
        throw new Error('return type is missing');
      }
      visitNode(node.typeRef, parent, nodeTable);

      // expr
      if (node.expr != null) {
        visitNode(node.expr, parent, nodeTable);
      }
      break;
    }
    case 'NumberLiteralNode': {
      break;
    }
    case 'ReferenceNode': {
      const semantic = parent.nameTable.get(node.name);
      if (semantic == null) {
        throw new Error(`unknown identifier: ${node.name}`);
      }
      break;
    }
    case 'TypeRefNode': {
      const semantic = parent.typeNameTable.get(node.name);
      if (semantic == null) {
        throw new Error(`unknown type name: ${node.name}`);
      }
      break;
    }
    case 'BinaryNode': {
      visitNode(node.left, parent, nodeTable);
      visitNode(node.right, parent, nodeTable);
      break;
    }
    case 'UnaryNode': {
      visitNode(node.expr, parent, nodeTable);
      break;
    }
    case 'IfNode': {
      visitNode(node.cond, parent, nodeTable);
      visitNode(node.thenExpr, parent, nodeTable);
      if (node.elseExpr != null) {
        visitNode(node.elseExpr, parent, nodeTable);
      }
      break;
    }
    case 'BlockNode': {
      const symbol = new BlockSymbol(parent);
      for (const child of node.body) {
        visitNode(child, symbol, nodeTable);
      }
      break;
    }
    case 'CallNode': {
      visitNode(node.expr, parent, nodeTable);
      for (const child of node.args) {
        visitNode(child, parent, nodeTable);
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
        visitNode(node.expr, parent, nodeTable);
      }
      break;
    }
    case 'AssignNode': {
      visitNode(node.target, parent, nodeTable);
      visitNode(node.expr, parent, nodeTable);
      break;
    }
    case 'WhileNode': {
      const symbol = new WhileSymbol(parent);
      visitNode(node.expr, parent, nodeTable);
      for (const child of node.body) {
        visitNode(child, symbol, nodeTable);
      }
      break;
    }
    case 'SwitchNode': {
      visitNode(node.expr, parent, nodeTable);
      for (const arm of node.arms) {
        visitNode(arm.cond, parent, nodeTable);
        visitNode(arm.thenBlock, parent, nodeTable);
      }
      if (node.defaultBlock != null) {
        visitNode(node.defaultBlock, parent, nodeTable);
      }
      break;
    }
    case 'ExpressionStatementNode': {
      visitNode(node.expr, parent, nodeTable);
      break;
    }
  }
}
