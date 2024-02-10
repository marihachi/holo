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
  const clangPath = findClangPath();
  if (clangPath == null) {
    throw new Error('clang command not found');
  }
  const linkItems: string[] = [];
  for (let i = 2; i < process.argv.length; i++) {
    const inputFilename = process.argv[i];
    const inputInfo = path.parse(inputFilename);
    let linkPath;
    switch (inputInfo.ext) {
      case '.ho': {
        let sourceCode;
        try {
          const inputPath = path.resolve(process.cwd(), inputFilename);
          sourceCode = fs.readFileSync(inputPath, { encoding: 'utf-8' });
        } catch (err) {
          throw new Error('Failed to load a source file.');
        }
        const ast = parse(sourceCode);
        const unitSymbol = resolve(ast);
        typecheck(ast, unitSymbol);
        const code = emit(unitSymbol);
        const outputPath = path.resolve(process.cwd(), `${inputInfo.name}.ll`);
        fs.writeFileSync(outputPath, code, { encoding: 'utf-8' });
        linkPath = outputPath;
        break;
      }
      case '.a': {
        const inputPath = path.resolve(process.cwd(), inputFilename);
        linkPath = inputPath;
        break;
      }
      default: {
        continue;
      }
    }
    linkItems.push(linkPath);
  }
  try {
    const outputPath = path.resolve(process.cwd(), 'main');
    childProcess.execSync(`${ clangPath } ${ linkItems.join(' ') } -o ${ outputPath }`);
  } catch(err) {
    console.log(err);
    throw new Error('compile failure.');
  }
}
entry();
