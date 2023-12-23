import { FunctionDeclNode, VariableDeclNode } from './syntax-node.js';
import { Type } from './type.js';

export type SemanticNode =
  | HoloFunction
  | Variable
  | HoloFunctionParam;

export class HoloFunction {
  kind = 'HoloFunction' as const;
  constructor(
    public name: string,
    public node: FunctionDeclNode,
    public type: Type | undefined,
  ) {}
}

export class Variable {
  kind = 'Variable' as const;
  constructor(
    public name: string,
    public node: VariableDeclNode,
    public type: Type | undefined,
  ) {}
}

export class HoloFunctionParam {
  kind = 'HoloFunctionParam' as const;
  constructor(
    public name: string,
    public type: Type | undefined,
  ) {}
}
