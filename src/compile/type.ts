export type Type =
  | PrimitiveType
  | PointerType
  | ArrayType
  | FunctionType;

export type PrimitiveKind = 'int' /* | 'uint' | 'bool' */ | 'void';

export class PrimitiveType {
  kind = 'PrimitiveType' as const;
  constructor(
    public primitiveKind: PrimitiveKind,
  ) {}
}

// int*   --> pointer(primitive(int))
// int[]  --> array(1, primitive(int))
// int[,] --> array(2, primitive(int))
// int**  --> pointer(pointer(primitive(int)))
// int*[] --> array(1, pointer(primitive(int)))
// int[]* --> pointer(array(1, primitive(int)))

export class PointerType {
  kind = 'PointerType' as const;
  constructor(
    public innerType: Type,
  ) {}
}

export class ArrayType {
  kind = 'ArrayType' as const;
  constructor(
    public innerType: Type,
    public dimensions: { size: number | undefined }[],
  ) {}
}

export class FunctionType {
  kind = 'FunctionType' as const;
  constructor(
    public paramTypes: Type[],
    public retType: Type,
  ) {}
}
