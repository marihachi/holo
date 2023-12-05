export enum TokenKind {
  EOF,
  NewLine,
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

  If,
  Else,
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