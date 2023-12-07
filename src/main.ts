import { inspect } from 'util';
import { generateCode } from './codegen/index.js';
import { analyze } from './semantic/index.js';
import { parse } from './syntax/parse.js';

function entry() {
  const ast = parse('fn abc(a, b, c) {}');
  console.log(inspect(ast, { depth: 10 }));
  //analyze(ast);
  //generateCode(ast);
}
entry();
