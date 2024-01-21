export type LowLevelNode = LowLevelModule | LowLevelFunction | LowLevelVariable | LowLevelBlock | LowLevelValue | LowLevelInst;

export class LowLevelModule {
  functions: LowLevelFunction[] = [];
  globalVars: LowLevelVariable[] = [];
}

export class LowLevelFunction {
  localVars: LowLevelVariable[] = [];
  registers: LowLevelRegister[] = [];
  blocks: LowLevelBlock[] = [];

  constructor(
    public entryBlock: LowLevelBlock,
    public returnType: string,
  ) {}
}

export class LowLevelVariable {
  id: string | undefined;

  constructor(
    public type: string,
  ) {}
}

export class LowLevelBlock {
  label: string | undefined;
  instructions: LowLevelInst[] = [];
}

export type LowLevelValue = LowLevelLiteral | LowLevelRegister | LowLevelPointer;

export class LowLevelLiteral {
  constructor(
    public type: string,
    public value: string,
  ) {}
}

export class LowLevelRegister {
  id: string | undefined;

  constructor(
    public type: string,
  ) {}
}

export class LowLevelPointer {
  constructor(
    public variable: LowLevelVariable,
  ) {}
}

export type LowLevelInst = AllocaInst | StoreInst | LoadInst | RetInst | BrInst;

export class AllocaInst {
  constructor(
    public target: LowLevelValue,
  ) {}
}

export class StoreInst {
  constructor(
    public source: LowLevelValue,
    public dest: LowLevelValue,
  ) {}
}

export class LoadInst {
  constructor(
    public source: LowLevelValue,
    public dest: LowLevelValue,
  ) {}
}

export class RetInst {
  constructor(
    public value: LowLevelValue | undefined,
  ) {}
}

export class BrInst {
  constructor(
    public cond: LowLevelValue | undefined,
    public thenLabel: LowLevelBlock | undefined,
    public elseLabel: LowLevelBlock | undefined,
  ) {}
}
