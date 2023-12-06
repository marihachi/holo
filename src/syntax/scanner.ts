import { CharStream } from './char-stream.js';
import { error } from './error.js';
import type { ITokenStream } from './token-stream.js';
import type { Token } from './token.js';
import { TOKEN, TokenKind } from './token.js';

const spaceChars = [' ', '\t'];
const lineBreakChars = ['\r', '\n'];
const digit = /^[0-9]$/;
const wordChar = /^[A-Za-z0-9_]$/;

/**
 * 入力文字列からトークンを読み取るクラス
*/
export class Scanner implements ITokenStream {
  private stream: CharStream;
  private _tokens: Token[] = [];

  constructor(source: string)
  constructor(stream: CharStream)
  constructor(x: string | CharStream) {
    if (typeof x === 'string') {
      this.stream = new CharStream(x);
    } else {
      this.stream = x;
    }
    this._tokens.push(this.readToken());
  }

  /**
   * カーソル位置にあるトークンを取得します。
  */
  public get token(): Token {
    return this._tokens[0]!;
  }

  /**
   * カーソル位置にあるトークンの種類を取得します。
  */
  public getKind(): TokenKind {
    return this.token.kind;
  }

  /**
   * カーソル位置を次のトークンへ進めます。
  */
  public next(): void {
    // 現在のトークンがEOFだったら次のトークンに進まない
    if (this._tokens[0]!.kind === TokenKind.EOF) {
      return;
    }

    this._tokens.shift();

    if (this._tokens.length === 0) {
      this._tokens.push(this.readToken());
    }
  }

  /**
   * トークンの先読みを行います。カーソル位置は移動されません。
  */
  public lookahead(offset: number): Token {
    while (this._tokens.length <= offset) {
      this._tokens.push(this.readToken());
    }

    return this._tokens[offset]!;
  }

  /**
   * カーソル位置にあるトークンが指定したトークンの種類と一致するかを確認します。
   * 一致しなかった場合には文法エラーを発生させます。
  */
  public expect(kind: TokenKind): void {
    if (this.getKind() !== kind) {
      throw error(`unexpected token: ${TokenKind[this.getKind()]}`, this.token.loc);
    }
  }

  /**
   * カーソル位置にあるトークンが指定したトークンの種類と一致することを確認し、
   * カーソル位置を次のトークンへ進めます。
  */
  public nextWith(kind: TokenKind): void {
    this.expect(kind);
    this.next();
  }

  private readToken(): Token {
    let token;

    while (true) {
      if (this.stream.eof) {
        token = TOKEN(TokenKind.EOF, this.stream.getPos(), { });
        break;
      }
      // skip spasing
      if (spaceChars.includes(this.stream.char)) {
        this.stream.next();
        continue;
      }

      // トークン位置を記憶
      const loc = this.stream.getPos();

      if (lineBreakChars.includes(this.stream.char)) {
        this.stream.next();
        token = TOKEN(TokenKind.NewLine, loc, { });
        return token;
      }
      switch (this.stream.char) {
        case '%': {
          this.stream.next();
          token = TOKEN(TokenKind.Percent, loc, { });
          break;
        }
        case '(': {
          this.stream.next();
          token = TOKEN(TokenKind.OpenParen, loc, { });
          break;
        }
        case ')': {
          this.stream.next();
          token = TOKEN(TokenKind.CloseParen, loc, { });
          break;
        }
        case '*': {
          this.stream.next();
          token = TOKEN(TokenKind.Asterisk, loc, { });
          break;
        }
        case '+': {
          this.stream.next();
          token = TOKEN(TokenKind.Plus, loc, { });
          break;
        }
        case '-': {
          this.stream.next();
          token = TOKEN(TokenKind.Minus, loc, { });
          break;
        }
        case '/': {
          this.stream.next();
          if (!this.stream.eof && (this.stream.char as string) === '*') {
            this.stream.next();
            this.skipCommentRange();
            continue;
          } else if (!this.stream.eof && (this.stream.char as string) === '/') {
            this.stream.next();
            this.skipCommentLine();
            continue;
          } else {
            token = TOKEN(TokenKind.Slash, loc, { });
          }
          break;
        }
        case ';': {
          this.stream.next();
          token = TOKEN(TokenKind.SemiColon, loc, { });
          break;
        }
        case '=': {
          this.stream.next();
          if (!this.stream.eof && (this.stream.char as string) === '=') {
            this.stream.next();
            token = TOKEN(TokenKind.Eq2, loc, { });
          } else {
            token = TOKEN(TokenKind.Eq, loc, { });
          }
          break;
        }
      }
      if (token == null) {
        const digitToken = this.tryReadDigits();
        if (digitToken) {
          token = digitToken;
          break;
        }
        const wordToken = this.tryReadWord();
        if (wordToken) {
          token = wordToken;
          break;
        }
        throw error(`invalid character: "${this.stream.char}"`, loc);
      }
      break;
    }
    return token;
  }

  private tryReadWord(): Token | undefined {
    // read a word
    let value = '';

    const loc = this.stream.getPos();

    while (!this.stream.eof && wordChar.test(this.stream.char)) {
      value += this.stream.char;
      this.stream.next();
    }
    if (value.length === 0) {
      return;
    }
    // check word kind
    switch (value) {
      case 'fn': {
        return TOKEN(TokenKind.Fn, loc, { });
      }
      case 'var': {
        return TOKEN(TokenKind.Var, loc, { });
      }
      case 'if': {
        return TOKEN(TokenKind.If, loc, { });
      }
      case 'else': {
        return TOKEN(TokenKind.Else, loc, { });
      }
      default: {
        return TOKEN(TokenKind.Identifier, loc, { value });
      }
    }
  }

  private tryReadDigits(): Token | undefined {
    let wholeNumber = '';
    let fractional = '';

    const loc = this.stream.getPos();

    while (!this.stream.eof && digit.test(this.stream.char)) {
      wholeNumber += this.stream.char;
      this.stream.next();
    }
    if (wholeNumber.length === 0) {
      return;
    }
    if (!this.stream.eof && this.stream.char === '.') {
      this.stream.next();
      while (!this.stream.eof as boolean && digit.test(this.stream.char as string)) {
        fractional += this.stream.char;
        this.stream.next();
      }
      if (fractional.length === 0) {
        throw error('digit expected', loc);
      }
    }
    let value;
    if (fractional.length > 0) {
      value = wholeNumber + '.' + fractional;
    } else {
      value = wholeNumber;
    }
    return TOKEN(TokenKind.NumberLiteral, loc, { value });
  }

  private skipCommentLine(): void {
    while (true) {
      if (this.stream.eof) {
        break;
      }
      if (this.stream.char === '\n') {
        this.stream.next();
        break;
      }
      this.stream.next();
    }
  }

  private skipCommentRange(): void {
    while (true) {
      if (this.stream.eof) {
        break;
      }
      if (this.stream.char === '*') {
        this.stream.next();
        if ((this.stream.char as string) === '/') {
          this.stream.next();
          break;
        }
        continue;
      }
      this.stream.next();
    }
  }
}
