import type { Loc } from '../ast.js';

export function error(message: string, loc: Loc) {
  return new Error(`${message} (${loc.line}:${loc.column})`);
}
