import { generateCode } from './codegen/index.js';
import { analyze } from './semantic/index.js';
import { parse } from './syntax/parse.js';

function entry() {
  const ast = parse('');
  analyze(ast);
  generateCode(ast);
}
entry();
