export enum TokenKind {
  EOF,
  Identifier,
  NumberLiteral,

  /** "%" */
  Percent,
  /** "(" */
  OpenParen,
  /** ")" */
  CloseParen,
  /** "*" */
  Asterisk,
  /** "+" */
  Plus,
  /** "-" */
  Minus,
  /** "/" */
  Slash,
  /** ";" */
  SemiColon,
  /** "=" */
  Eq,
  /** "==" */
  Eq2,

  If,
  Else,
  Fn,
  Var,
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
