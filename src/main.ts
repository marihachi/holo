import fs from 'fs';
import path from 'path';
import childProcess from 'child_process';
import { emit } from './compile/emit.js';
import { typecheck } from './compile/typecheck.js';
import { parse } from './compile/parse.js';
import { resolve } from './compile/resolve.js';

const binDirs = ['/bin/', '/sbin/', '/usr/bin/', '/usr/sbin/', '/usr/local/bin/', '/usr/local/sbin/'];

function findClangPath(): string | undefined {
  for (const dir of binDirs) {
    try {
      const clangPath = dir + 'clang';
      fs.accessSync(clangPath);
      return clangPath;
    } catch {}
  }
  return undefined;
}

function entry() {
  if (process.argv.length < 3) {
    throw new Error('no input files');
  }
  const filepath = process.argv[2];
  const fileInfo = path.parse(filepath);

  const clangPath = findClangPath();
  if (clangPath == null) {
    throw new Error('clang command not found');
  }

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
  const llCodePath = path.resolve(fileInfo.dir, `${fileInfo.name}.ll`);
  fs.writeFileSync(llCodePath, code, { encoding: 'utf-8' });

  try {
    childProcess.execSync(`${clangPath} ${llCodePath} -o ${path.resolve(fileInfo.dir, fileInfo.name)}`);
  } catch(err) {
    console.log('Failed to compile.');
  }
}
entry();
