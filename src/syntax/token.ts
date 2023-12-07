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
  PercentEq,
  /** "&" */
  And,
  /** "&&" */
  And2,
  /** "&=" */
  AndEq,
  /** "(" */
  OpenParen,
  /** ")" */
  CloseParen,
  /** "*" */
  Asterisk,
  /** "*=" */
  AsterEq,
  /** "+" */
  Plus,
  /** "+=" */
  PlusEq,
  /** "," */
  Comma,
  /** "-" */
  Minus,
  /** "-=" */
  MinusEq,
  /** "/" */
  Slash,
  /** "/=" */
  SlashEq,
  /** ":" */
  Colon,
  /** ";" */
  SemiColon,
  /** "<" */
  Lt,
  /** "<=" */
  LtEq,
  /** "<<" */
  Lt2,
  /** "<<=" */
  Lt2Eq,
  /** "=" */
  Eq,
  /** "==" */
  Eq2,
  /** ">" */
  Gt,
  /** ">=" */
  GtEq,
  /** ">>" */
  Gt2,
  /** ">>=" */
  Gt2Eq,
  /** "^" */
  Hat,
  /** "^=" */
  HatEq,
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
  /** "|=" */
  OrEq,
  /** "}" */
  CloseBrace,

  Break,
  Continue,
  Do,
  Else,
  Fn,
  If,
  Return,
  Switch,
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
