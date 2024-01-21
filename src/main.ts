import fs from 'fs';
import path from 'path';
import { emit } from './compile/emit-llvm.js';
import { typecheck } from './compile/typecheck.js';
import { parse } from './compile/parse.js';
import { resolve } from './compile/resolve.js';

function entry() {
  if (process.argv.length < 3) {
    throw new Error('no input files');
  }
  const filepath = process.argv[2];
  const fileInfo = path.parse(filepath);

  // load file
  let sourceCode;
  try {
    sourceCode = fs.readFileSync(filepath, { encoding: 'utf-8' });
  } catch (err) {
    throw new Error('Failed to load a source file.');
  }

  let ast = parse(sourceCode);
  const unitSymbol = resolve(ast);
  typecheck(ast, unitSymbol);
  const code = emit(unitSymbol);
  fs.writeFileSync(path.join(fileInfo.dir, `${fileInfo.name}.ll`), code, { encoding: 'utf-8' });
}
entry();
