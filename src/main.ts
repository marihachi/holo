import fs from 'fs';
import { inspect } from 'util';
import { Emitter, generateCode } from './compile/codegen.js';
import { typecheck } from './compile/typecheck.js';
import { parse } from './compile/parse.js';

function entry() {
  // load file
  let sourceCode;
  try {
    sourceCode = fs.readFileSync('test.ho', { encoding: 'utf8' });
  } catch (err) {
    throw new Error('Failed to load a source file.');
  }

  // parse
  const ast = parse(sourceCode);
  //console.log(inspect(ast, { depth: 10 }));
  typecheck(ast);
  const code = generateCode(ast);
  console.log(code);
}
entry();
