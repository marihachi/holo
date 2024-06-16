namespace Holoc.Compile.Syntax;

public class TokenReader
{
    private StreamReader? Stream;
    private char? CurrentChar;

    public int Column { get; private set; }
    public int Line { get; private set; }
    public SyntaxToken? Token { get; private set; }
    public string? Error { get; private set; }

    public TokenKind TokenKind => Token.Kind;

    private void ReadChar()
    {
        if (Stream!.EndOfStream)
        {
            CurrentChar = null;
            return;
        }

        var ch = (char)Stream.Read();

        if (ch == '\0')
        {
            CurrentChar = null;
            return;
        }

        CurrentChar = ch;
    }

    private char? PeekChar()
    {
        if (Stream!.EndOfStream)
        {
            return null;
        }

        var ch = (char)Stream.Peek();

        if (ch == '\0')
        {
            return null;
        }

        return ch;
    }

    public void Initialize(Stream stream)
    {
        Stream = new StreamReader(stream);
        CurrentChar = null;
        Column = 1;
        Line = 1;
        Token = null;
        Error = null;
    }

    public string CreateUnexpectedError()
    {
        if (TokenKind == TokenKind.Word)
        {
            return $"Unexpected token: {(string)Token!.Value!} {Token.Location.Line}:{Token.Location.Column}";
        }

        return $"Unexpected token: {Token!.Kind} {Token.Location.Line}:{Token.Location.Column}";
    }

    /// <summary>
    /// トークンを読み取ります。
    /// 読み取りに失敗した場合は、Errorプロパティにエラーの内容が設定されます。
    /// </summary>
    public bool Read()
    {
        ReadInternal();
        return Token != null;
    }

    private void ReadInternal()
    {
        // NOTE: ReadCharしたら必ず位置の更新を行う。

        if (Stream == null)
        {
            throw new InvalidOperationException("not initialized");
        }

        Token = null;

        while (true)
        {
            TokenLocation beginLocation;

            // 1文字読み取る
            ReadChar();

            // ストリームの終わりに達していたら
            if (CurrentChar == null)
            {
                Token = new SyntaxToken(TokenKind.EOF, new TokenLocation(Column, Line));
                return;
            }

            switch (CurrentChar)
            {
                case ' ':
                    Column += 1;
                    continue;

                case '\t':
                    Column += 1;
                    continue;

                // LF
                case '\n':
                    Column = 1;
                    Line += 1;
                    continue;

                // CR
                case '\r':
                    // LFが続いていたら一緒に消費する
                    if (PeekChar() == '\n')
                    {
                        ReadChar();
                    }
                    Column = 1;
                    Line += 1;
                    continue;

                case '*':
                    beginLocation = new TokenLocation(Column, Line);
                    Column += 1;

                    // 先読み
                    if (PeekChar() == '=')
                    {
                        ReadChar();
                        Column += 1;
                        Token = new SyntaxToken(TokenKind.AsterEq, beginLocation);
                        return;
                    }

                    Token = new SyntaxToken(TokenKind.Asterisk, beginLocation);
                    return;

                case '+':
                    beginLocation = new TokenLocation(Column, Line);
                    Column += 1;

                    // 先読み
                    if (PeekChar() == '=')
                    {
                        ReadChar();
                        Column += 1;
                        Token = new SyntaxToken(TokenKind.PlusEq, beginLocation);
                        return;
                    }

                    Token = new SyntaxToken(TokenKind.Plus, beginLocation);
                    return;

                case '-':
                    beginLocation = new TokenLocation(Column, Line);
                    Column += 1;

                    // 先読み
                    if (PeekChar() == '=')
                    {
                        ReadChar();
                        Column += 1;
                        Token = new SyntaxToken(TokenKind.MinusEq, beginLocation);
                        return;
                    }

                    Token = new SyntaxToken(TokenKind.Minus, beginLocation);
                    return;

                case '/':
                    beginLocation = new TokenLocation(Column, Line);
                    Column += 1;

                    // 先読み
                    if (PeekChar() == '=')
                    {
                        ReadChar();
                        Column += 1;
                        Token = new SyntaxToken(TokenKind.SlashEq, beginLocation);
                        return;
                    }

                    Token = new SyntaxToken(TokenKind.Slash, beginLocation);
                    return;
            }

            // 数字
            if (CurrentChar >= '0' && CurrentChar <= '9')
            {
                beginLocation = new TokenLocation(Column, Line);
                string wholeNumber = "";

                wholeNumber += CurrentChar;
                Column += 1;

                // 後続の文字を読む
                while (true)
                {
                    var ch = PeekChar();
                    if (ch == null || !(ch >= '0' && ch <= '9')) break;

                    ReadChar();
                    wholeNumber += ch;
                    Column += 1;
                }

                string value = wholeNumber;
                Token = new SyntaxToken(TokenKind.NumberLiteral, beginLocation, value);
                return;
            }

            // 識別子またはキーワード
            if (CurrentChar >= 'A' && CurrentChar <= 'Z' ||
                CurrentChar >= 'a' && CurrentChar <= 'z' ||
                CurrentChar == '_')
            {
                beginLocation = new TokenLocation(Column, Line);
                string value = "";

                value += CurrentChar;
                Column += 1;

                // 後続の文字を読む
                while (true)
                {
                    var ch = PeekChar();
                    if (ch == null || !(
                        ch >= '0' && ch <= '9' ||
                        ch >= 'A' && ch <= 'Z' ||
                        ch >= 'a' && ch <= 'z' ||
                        ch == '_'))
                    {
                        break;
                    }

                    ReadChar();
                    value += ch;
                    Column += 1;
                }

                Token = new SyntaxToken(TokenKind.Word, beginLocation, value);
                return;
            }

            Error = $"Unexpected char: '{CurrentChar}' {Line}:{Column}";
            return;
        }
    }

    public void Peek()
    {
        throw new NotImplementedException();
    }
}
