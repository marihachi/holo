import { FunctionDecl, VariableDecl } from './ast.js';

export type Type =
  | PremitiveType
  | PointerType
  | ArrayType;

export type PrimitiveKind = 'int' | 'bool';

export class PremitiveType {
  kind = 'PremitiveType' as const;
  constructor(
    public primitiveKind: PrimitiveKind,
  ) {}
}

// int* --> pointer(primitive(int))
// int[] --> array(1, primitive(int))
// int[,] --> array(2, primitive(int))
// int** --> pointer(pointer(primitive(int)))
// int*[] --> array(1, pointer(primitive(int)))
// int[]* --> pointer(array(1, primitive(int)))

export class PointerType {
  kind = 'PointerType' as const;
  constructor(
    public innerType: Type | undefined,
  ) {}
}

export class ArrayType {
  kind = 'ArrayType' as const;
  constructor(
    public innerType: Type | undefined,
  ) {}
}
