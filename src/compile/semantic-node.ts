import { BlockNode, FuncParameterNode, FunctionDeclNode, SyntaxNode, UnitNode, VariableDeclNode, WhileNode } from './syntax-node.js';
import { Type } from './type.js';

export class SymbolTable<T> {
  table: Map<T, SemanticSymbol> = new Map();

  constructor(
    public parent: SymbolTable<T> | undefined,
  ) {}

  set(key: T, symbol: SemanticSymbol): void {
    this.table.set(key, symbol);
  }

  get(key: T): SemanticSymbol | undefined {
    const symbol = this.table.get(key);
    if (symbol != null) {
      return symbol;
    }
    return this.parent?.get(key);
  }
}

export type SemanticSymbol =
  | UnitSymbol
  | FunctionSymbol
  | VariableSymbol
  | FunctionParamSymbol
  | TypeSymbol
  | BlockSymbol
  | WhileSymbol;

export type ContainerSymbol =
  | UnitSymbol
  | FunctionSymbol
  | BlockSymbol
  | WhileSymbol;

export class UnitSymbol {
  kind = 'UnitSymbol' as const;
  nameTable: SymbolTable<string> = new SymbolTable(undefined);
  typeNameTable: SymbolTable<string> = new SymbolTable(undefined);
  nodeTable: SymbolTable<SyntaxNode> = new SymbolTable(undefined);
  constructor(
    public node: UnitNode,
  ) {}
}

export class FunctionSymbol {
  kind = 'FunctionSymbol' as const;
  nameTable: SymbolTable<string>;
  typeNameTable: SymbolTable<string>;
  constructor(
    public name: string,
    parent: ContainerSymbol,
    public node: FunctionDeclNode,
  ) {
    this.nameTable = new SymbolTable(parent.nameTable);
    this.typeNameTable = new SymbolTable(parent.typeNameTable);
  }
}

export class VariableSymbol {
  kind = 'VariableSymbol' as const;
  registerName: string | undefined;
  constructor(
    public name: string,
    public node: VariableDeclNode,
  ) {}
}

export class FunctionParamSymbol {
  kind = 'FunctionParamSymbol' as const;
  constructor(
    public name: string,
    public node: FuncParameterNode,
  ) {}
}

export class TypeSymbol {
  kind = 'TypeSymbol' as const;
  constructor(
    public name: string,
    public type: Type | undefined,
  ) {}
}

export class BlockSymbol {
  kind = 'BlockSymbol' as const;
  nameTable: SymbolTable<string>;
  typeNameTable: SymbolTable<string>;
  constructor(
    parent: ContainerSymbol,
    public node: BlockNode,
  ) {
    this.nameTable = new SymbolTable(parent.nameTable);
    this.typeNameTable = new SymbolTable(parent.typeNameTable);
  }
}

export class WhileSymbol {
  kind = 'WhileSymbol' as const;
  nameTable: SymbolTable<string>;
  typeNameTable: SymbolTable<string>;
  constructor(
    parent: ContainerSymbol,
    public node: WhileNode,
  ) {
    this.nameTable = new SymbolTable(parent.nameTable);
    this.typeNameTable = new SymbolTable(parent.typeNameTable);
  }
}
