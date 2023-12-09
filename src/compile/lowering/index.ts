import { Unit } from '../ast.js';
import { loweringSwitch } from './lowering-switch.js';

export function lowering(node: Unit): Unit {
  node = loweringSwitch(node);
  return node;
}
