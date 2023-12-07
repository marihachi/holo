export enum TokenKind {
  EOF,
  Identifier,
  NumberLiteral,

  /** "!" */
  Not,
  /** "!=" */
  NotEq,
  /** "%" */
  Percent,
  /** "%=" */
  RemAssign,
  /** "&" */
  And,
  /** "&&" */
  And2,
  /** "(" */
  OpenParen,
  /** ")" */
  CloseParen,
  /** "*" */
  Asterisk,
  /** "/=" */
  MulAssign,
  /** "+" */
  Plus,
  /** "+=" */
  AddAssign,
  /** "," */
  Comma,
  /** "-" */
  Minus,
  /** "-=" */
  SubAssign,
  /** "/" */
  Slash,
  /** "/=" */
  DivAssign,
  /** ":" */
  Colon,
  /** ";" */
  SemiColon,
  /** "<" */
  Lt,
  /** "<=" */
  Lte,
  /** "=" */
  Eq,
  /** "==" */
  Eq2,
  /** ">" */
  Gt,
  /** ">=" */
  Gte,
  /** "^" */
  Hat,
  /** "[" */
  OpenBracket,
  /** "]" */
  CloseBracket,
  /** "{" */
  OpenBrace,
  /** "|" */
  Or,
  /** "||" */
  Or2,
  /** "}" */
  CloseBrace,

  Break,
  Continue,
  Do,
  Else,
  Fn,
  If,
  Return,
  Var,
  While,
}

export type TokenLocation = { column: number, line: number };

export class Token {
  constructor(
    public kind: TokenKind,
    public loc: TokenLocation,
    public value?: string,
  ) { }
}

export function TOKEN(kind: TokenKind, loc: TokenLocation, opts?: { value?: Token['value'] }): Token {
  return new Token(kind, loc, opts?.value);
}
