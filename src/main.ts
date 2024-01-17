import fs from 'fs';
import { inspect } from 'util';
import { emit } from './compile/emit-llvm.js';
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
  ast = lowering(ast);
  // console.log(inspect(ast, { depth: 30 }));
  // console.log('----');
  const unitSymbol = resolve(ast);
  typecheck(ast, unitSymbol);
  const code = emit(unitSymbol);
  // console.log(code);
  fs.writeFileSync('obj/test.ll', code, { encoding: 'utf-8' });
}
entry();
