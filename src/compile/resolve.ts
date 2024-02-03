import { SyntaxNode, UnitNode } from './syntax-node.js';
import { PrimitiveType } from './type.js';
import {
  BlockSymbol, ContainerSymbol, FunctionParamSymbol, FunctionSymbol, SymbolTable, TypeSymbol, UnitSymbol,
  VariableSymbol, WhileSymbol
} from './semantic-node.js';

// 宣言ノードに対してsemantic nodeを生成します。
// 参照ノードの名前を解決しsemantic nodeと関連付けます。

export function resolve(ast: UnitNode): UnitSymbol {
  const symbol = new UnitSymbol(ast);
  setPrimitiveTypes(symbol);
  for (const child of ast.decls) {
    resolveNode(child, symbol, symbol.nodeTable);
  }
  return symbol;
}

function setPrimitiveTypes(symbol: ContainerSymbol) {
  symbol.typeNameTable.set('int', new TypeSymbol('int', new PrimitiveType('int')));
  symbol.typeNameTable.set('uint', new TypeSymbol('uint', new PrimitiveType('uint')));
  symbol.typeNameTable.set('bool', new TypeSymbol('bool', new PrimitiveType('bool')));
}

function resolveNode(node: SyntaxNode, parent: ContainerSymbol, nodeTable: SymbolTable<SyntaxNode>): void {
  switch (node.kind) {
    case 'FunctionDeclNode': {
      if (parent.nameTable.get(node.name) != null) {
        throw new Error(`identifier "${node.name}" is already exists`);
      }

      const symbol = new FunctionSymbol(node.name, parent, node);
      parent.nameTable.set(node.name, symbol);
      nodeTable.set(node, symbol);

      // return type
      if (node.typeRef == null) {
        throw new Error('return type is missing');
      }
      resolveNode(node.typeRef, symbol, nodeTable);

      // parameters
      for (const parameter of node.parameters) {
        resolveNode(parameter, symbol, nodeTable);
      }

      // body
      for (const child of node.body) {
        resolveNode(child, symbol, nodeTable);
      }
      break;
    }
    case 'FuncParameterNode': {
      if (parent.nameTable.get(node.name) != null) {
        throw new Error(`identifier "${node.name}" is already exists`);
      }

      const symbol = new FunctionParamSymbol(node.name, node);
      parent.nameTable.set(node.name, symbol);
      nodeTable.set(node, symbol);

      // type
      if (node.typeRef == null) {
        throw new Error('parameter type is missing');
      }
      resolveNode(node.typeRef, parent, nodeTable);
      break;
    }
    case 'VariableDeclNode': {
      if (parent.nameTable.get(node.name) != null) {
        throw new Error(`identifier "${node.name}" is already exists`);
      }

      const symbol = new VariableSymbol(node.name, node);
      parent.nameTable.set(node.name, symbol);
      nodeTable.set(node, symbol);

      // type
      if (node.typeRef == null) {
        throw new Error('return type is missing');
      }
      resolveNode(node.typeRef, parent, nodeTable);

      // expr
      if (node.expr != null) {
        resolveNode(node.expr, parent, nodeTable);
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
      nodeTable.set(node, semantic);
      break;
    }
    case 'TypeRefNode': {
      const semantic = parent.typeNameTable.get(node.name);
      if (semantic == null) {
        throw new Error(`unknown type name: ${node.name}`);
      }
      nodeTable.set(node, semantic);
      break;
    }
    case 'BinaryNode': {
      resolveNode(node.left, parent, nodeTable);
      resolveNode(node.right, parent, nodeTable);
      break;
    }
    case 'UnaryNode': {
      resolveNode(node.expr, parent, nodeTable);
      break;
    }
    case 'IfNode': {
      resolveNode(node.cond, parent, nodeTable);
      resolveNode(node.thenExpr, parent, nodeTable);
      if (node.elseExpr != null) {
        resolveNode(node.elseExpr, parent, nodeTable);
      }
      break;
    }
    case 'BlockNode': {
      const symbol = new BlockSymbol(parent, node);
      for (const child of node.body) {
        resolveNode(child, symbol, nodeTable);
      }
      break;
    }
    case 'CallNode': {
      resolveNode(node.expr, parent, nodeTable);
      for (const child of node.args) {
        resolveNode(child, parent, nodeTable);
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
        resolveNode(node.expr, parent, nodeTable);
      }
      break;
    }
    case 'AssignNode': {
      if (node.target.kind != 'ReferenceNode') {
        throw new Error(`invalid assign target`);
      }
      resolveNode(node.target, parent, nodeTable);
      resolveNode(node.expr, parent, nodeTable);
      break;
    }
    case 'WhileNode': {
      const symbol = new WhileSymbol(parent, node);
      resolveNode(node.expr, parent, nodeTable);
      for (const child of node.body) {
        resolveNode(child, symbol, nodeTable);
      }
      break;
    }
    case 'SwitchNode': {
      resolveNode(node.expr, parent, nodeTable);
      for (const arm of node.arms) {
        resolveNode(arm.cond, parent, nodeTable);
        resolveNode(arm.thenBlock, parent, nodeTable);
      }
      if (node.defaultBlock != null) {
        resolveNode(node.defaultBlock, parent, nodeTable);
      }
      break;
    }
    case 'ExpressionStatementNode': {
      resolveNode(node.expr, parent, nodeTable);
      break;
    }
  }
}
