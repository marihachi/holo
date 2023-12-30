import fs from 'fs';
import { inspect } from 'util';
import { emit } from './compile/emit.js';
import { typecheck } from './compile/typecheck.js';
import { parse } from './compile/parse.js';
import { lowering } from './compile/lowering.js';
import { resolve } from './compile/resolve.js';

function entry() {
  // load file
  let sourceCode;
  try {
    sourceCode = fs.readFileSync('test.ho', { encoding: 'utf8' });
  } catch (err) {
    throw new Error('Failed to load a source file.');
  }

  let ast = parse(sourceCode);
  // console.log(inspect(ast, { depth: 30 }));
  // console.log('----');
  const unitSymbol = resolve(ast);
  // console.log(inspect(semanticTable, { depth: 5 }));
  typecheck(ast, unitSymbol);
  ast = lowering(ast, unitSymbol);
  // console.log(inspect(ast, { depth: 30 }));
  // console.log('----');
  const code = emit(ast, unitSymbol);
  // console.log(code);
}
entry();
