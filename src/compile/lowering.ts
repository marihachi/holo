import { Unit } from './ast.js';

export function lowering(node: Unit): Unit {
  node = loweringSwitch(node);
  return node;
}

function loweringSwitch(node: Unit): Unit {
  // TODO
  return node;
}
