import fs from 'fs';
import { inspect } from 'util';
import { generate } from './compile/codegen.js';
import { typecheck } from './compile/typecheck.js';
import { parse } from './compile/parse.js';
import { lowering } from './compile/lowering.js';

function entry() {
  // load file
  let sourceCode;
  try {
    sourceCode = fs.readFileSync('test.ho', { encoding: 'utf8' });
  } catch (err) {
    throw new Error('Failed to load a source file.');
  }

  let ast = parse(sourceCode);
  console.log(inspect(ast, { depth: 10 }));
  console.log('----');
  typecheck(ast);
  ast = lowering(ast);
  console.log(inspect(ast, { depth: 10 }));
  console.log('----');
  const code = generate(ast);
  console.log(code);
}
entry();
