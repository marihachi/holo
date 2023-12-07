import { Assign, Block, DoWhile, Expression, FunctionDecl, If, Statement, Unit, VariableDecl, While } from './node.js';
import { Scanner } from './scanner.js';
import { ITokenStream } from './stream/token-stream.js';
import { TokenKind } from './token.js';

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

function parseParams(s: ITokenStream): string[] {
  s.nextWith(TokenKind.OpenParen);
  const items = [];
  while (s.getKind() != TokenKind.CloseParen) {
    if (items.length > 0) {
      s.nextWith(TokenKind.Comma);
    }
    s.expect(TokenKind.Identifier);
    const name = s.getToken().value!;
    s.next();
    items.push(name);
  }
  s.nextWith(TokenKind.CloseParen);
  return items;
}

function parseCond(s: ITokenStream): Expression {
  s.nextWith(TokenKind.OpenParen);
  const expr = parseExpr(s);
  s.nextWith(TokenKind.CloseParen);
  return expr;
}

function parseBlock(s: ITokenStream): (Statement | Expression)[] {
  s.nextWith(TokenKind.OpenBrace);
  const steps = [];
  while (s.getKind() != TokenKind.CloseBrace) {
    steps.push(parseStep(s));
  }
  s.nextWith(TokenKind.CloseBrace);
  return steps;
}

function parseStep(s: ITokenStream): Statement | Expression {
  // NOTE: セミコロンの有無でStatementかExpressionかは変わる
  // NOTE: 例外的にBlock式はセミコロンが無くてもステートメントとして扱える。セミコロンをつけることもできるが意味は同じ。
  // TODO: Expression
  // TODO: Statement
  // TODO: Assign
  throw new Error('todo');
}

function parseExpr(s: ITokenStream): Expression {
  throw new Error('todo');
}

function parseFunctionDecl(s: ITokenStream): FunctionDecl {
  const loc = s.getToken().loc;
  s.nextWith(TokenKind.Fn);

  s.expect(TokenKind.Identifier);
  const name = s.getToken().value!;
  s.next();

  const params = parseParams(s);
  const body = parseBlock(s);

  return new FunctionDecl(name, params, body, loc);
}

function parseVariableDecl(s: ITokenStream): VariableDecl {
  const loc = s.getToken().loc;
  s.nextWith(TokenKind.Var);

  s.expect(TokenKind.Identifier);
  const name = s.getToken().value!;
  s.next();

  let body;

  s.nextWith(TokenKind.SemiColon);
  return new VariableDecl(name, body, loc);
}

function parseIf(s: ITokenStream): If {
  s.nextWith(TokenKind.If);
  const cond = parseCond(s);
  const thenBlock = parseBlock(s);
  let elseBlock;
  if (s.getKind() == TokenKind.Else) {
    s.next();
    // TODO: else if
    elseBlock = parseBlock(s);
  }

  throw new Error('todo');
}

function parseWhile(s: ITokenStream): While {
  s.nextWith(TokenKind.While);
  const cond = parseCond(s);
  const body = parseBlock(s);
  throw new Error('todo');
}

function parseDoWhile(s: ITokenStream): DoWhile {
  s.nextWith(TokenKind.Do);
  const body = parseBlock(s);
  s.nextWith(TokenKind.While);
  const cond = parseCond(s);
  throw new Error('todo');
}
