import { generateCode } from './codegen/index.js';
import { analyze } from './semantic/index.js';
import { parse } from './syntax/index.js';

function entry() {
  const ast = parse('');
  analyze(ast);
  generateCode(ast);
}
entry();
