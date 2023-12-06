import { Assign, Block, DoWhile, Expression, FunctionDecl, If, Statement, Unit, VariableDecl, While } from './node.js';
import { Scanner } from './scanner.js';
import { ITokenStream } from './stream/token-stream.js';
import { TokenKind } from './token.js';

// NOTE: セミコロンの有無でStatementかExpressionかは変わるため、これらは一つの関数で処理する。
// NOTE: 例外的にBlock式はセミコロンが無くてもステートメントとして扱える。セミコロンをつけることもできるが意味は同じ。

export function parse(input: string): Unit {
  const s = new Scanner(input);

  const decls = [];
  while (s.getKind() != TokenKind.EOF) {
    switch (s.getKind()) {
      case TokenKind.Fn: {
        decls.push(parseFunctionDecl(s));
        break;
      }
      case TokenKind.Var: {
        decls.push(parseVariableDecl(s));
      }
    }
  }

  return new Unit(decls, { line: 1, column: 1 });
}

function parseStep(s: ITokenStream): Statement | Expression {
  // TODO: Expression
  // TODO: Statement
  // TODO: Assign
  throw new Error('todo');
}

function parseFunctionDecl(s: ITokenStream): FunctionDecl {
  s.nextWith(TokenKind.Fn);
  throw new Error('todo');
}

function parseVariableDecl(s: ITokenStream): VariableDecl {
  s.nextWith(TokenKind.Var);
  throw new Error('todo');
}

function parseIf(s: ITokenStream): If {
  s.nextWith(TokenKind.If);
  throw new Error('todo');
}

function parseBlock(s: ITokenStream): Block {
  throw new Error('todo');
}

function parseWhile(s: ITokenStream): While {
  s.nextWith(TokenKind.While);
  throw new Error('todo');
}

function parseDoWhile(s: ITokenStream): DoWhile {
  s.nextWith(TokenKind.Do);
  throw new Error('todo');
}
