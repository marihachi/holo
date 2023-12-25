import fs from 'fs';
import { inspect } from 'util';
import { generate } from './compile/codegen.js';
import { typecheck } from './compile/typecheck.js';
import { parse } from './compile/parse.js';
import { lowering } from './compile/lowering.js';
import { bind, SemanticTable } from './compile/bind.js';

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
  const semanticTable = new SemanticTable();
  bind(ast, semanticTable);
  // console.log(inspect(semanticTable, { depth: 5 }));
  typecheck(ast, semanticTable);
  ast = lowering(ast, semanticTable);
  // console.log(inspect(ast, { depth: 30 }));
  // console.log('----');
  const code = generate(ast, semanticTable);
  // console.log(code);
}
entry();
