namespace Holoc.Compile.Syntax.Token;

public class TokenReader
{
    private StreamReader? Stream;
    private char? CurrentChar;

    public int Column {
        get
        {
            return _Column;
        }

        private set
        {
            _Column = value;
            //Console.WriteLine($"{Line}:{Column}");
        }
    }
    private int _Column;

    public int Line
    {
        get
        {
            return _Line;
        }

        private set
        {
            _Line = value;
        }
    }
    private int _Line;

    public SyntaxToken? CurrentToken { get; private set; }

    // NextTokenはSetNextTokenから設定する。CurrentTokenへの反映が必要なため。
    public SyntaxToken? NextToken { get; private set; }
    
    public string? Error { get; private set; }

    public void Initialize(StreamReader stream)
    {
        Stream = stream;
        CurrentChar = null;
        Line = 1;
        Column = 1;
        CurrentToken = null;
        NextToken = null;
        Error = null;
    }

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

    private void SetNextToken(SyntaxToken token)
    {
        CurrentToken = NextToken;
        NextToken = token;
    }

    /// <summary>
    /// トークンを読み取ります。
    /// 読み取りに失敗した場合は、Errorプロパティにエラーの内容が設定されます。
    /// </summary>
    public bool Read()
    {
        ReadInternal();
        return NextToken != null;
    }

    private void ReadInternal()
    {
        // NOTE: ReadCharしたら必ず位置の更新を行う。

        if (Stream == null)
        {
            throw new InvalidOperationException("not initialized");
        }

        while (true)
        {
            TokenLocation beginLocation;

            // 1文字読み取る
            ReadChar();

            // ストリームの終わりに達していたら
            if (CurrentChar == null)
            {
                SetNextToken(new SyntaxToken(TokenKind.EOF, new TokenLocation(Column, Line), new TokenLocation(Column, Line)));
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

                case '{':
                    beginLocation = new TokenLocation(Column, Line);
                    Column += 1;
                    SetNextToken(new SyntaxToken(TokenKind.OpenBrace, beginLocation, new TokenLocation(Column, Line)));
                    return;

                case '}':
                    beginLocation = new TokenLocation(Column, Line);
                    Column += 1;
                    SetNextToken(new SyntaxToken(TokenKind.CloseBrace, beginLocation, new TokenLocation(Column, Line)));
                    return;

                case '(':
                    beginLocation = new TokenLocation(Column, Line);
                    Column += 1;
                    SetNextToken(new SyntaxToken(TokenKind.OpenParen, beginLocation, new TokenLocation(Column, Line)));
                    return;

                case ')':
                    beginLocation = new TokenLocation(Column, Line);
                    Column += 1;
                    SetNextToken(new SyntaxToken(TokenKind.CloseParen, beginLocation, new TokenLocation(Column, Line)));
                    return;

                case ',':
                    beginLocation = new TokenLocation(Column, Line);
                    Column += 1;
                    SetNextToken(new SyntaxToken(TokenKind.Comma, beginLocation, new TokenLocation(Column, Line)));
                    return;

                case '=':
                    beginLocation = new TokenLocation(Column, Line);
                    Column += 1;
                    SetNextToken(new SyntaxToken(TokenKind.Eq, beginLocation, new TokenLocation(Column, Line)));
                    return;

                case ':':
                    beginLocation = new TokenLocation(Column, Line);
                    Column += 1;
                    SetNextToken(new SyntaxToken(TokenKind.Colon, beginLocation, new TokenLocation(Column, Line)));
                    return;

                case ';':
                    beginLocation = new TokenLocation(Column, Line);
                    Column += 1;
                    SetNextToken(new SyntaxToken(TokenKind.SemiColon, beginLocation, new TokenLocation(Column, Line)));
                    return;

                case '*':
                    beginLocation = new TokenLocation(Column, Line);
                    Column += 1;

                    // 先読み
                    if (PeekChar() == '=')
                    {
                        ReadChar();
                        Column += 1;
                        SetNextToken(new SyntaxToken(TokenKind.AsterEq, beginLocation, new TokenLocation(Column, Line)));
                        return;
                    }

                    SetNextToken(new SyntaxToken(TokenKind.Asterisk, beginLocation, new TokenLocation(Column, Line)));
                    return;

                case '+':
                    beginLocation = new TokenLocation(Column, Line);
                    Column += 1;

                    // 先読み
                    if (PeekChar() == '=')
                    {
                        ReadChar();
                        Column += 1;
                        SetNextToken(new SyntaxToken(TokenKind.PlusEq, beginLocation, new TokenLocation(Column, Line)));
                        return;
                    }

                    SetNextToken(new SyntaxToken(TokenKind.Plus, beginLocation, new TokenLocation(Column, Line)));
                    return;

                case '-':
                    beginLocation = new TokenLocation(Column, Line);
                    Column += 1;

                    // 先読み
                    if (PeekChar() == '=')
                    {
                        ReadChar();
                        Column += 1;
                        SetNextToken(new SyntaxToken(TokenKind.MinusEq, beginLocation, new TokenLocation(Column, Line)));
                        return;
                    }

                    SetNextToken(new SyntaxToken(TokenKind.Minus, beginLocation, new TokenLocation(Column, Line)));
                    return;

                case '/':
                    beginLocation = new TokenLocation(Column, Line);
                    Column += 1;

                    // 先読み
                    if (PeekChar() == '=')
                    {
                        ReadChar();
                        Column += 1;
                        SetNextToken(new SyntaxToken(TokenKind.SlashEq, beginLocation, new TokenLocation(Column, Line)));
                        return;
                    }

                    SetNextToken(new SyntaxToken(TokenKind.Slash, beginLocation, new TokenLocation(Column, Line)));
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

                int value = int.Parse(wholeNumber);
                SetNextToken(new SyntaxToken(TokenKind.NumberLiteral, beginLocation, new TokenLocation(Column, Line), value));
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

                SetNextToken(new SyntaxToken(TokenKind.Word, beginLocation, new TokenLocation(Column, Line), value));
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

    public string CreateUnexpectedError()
    {
        if (NextToken!.Kind == TokenKind.Word)
        {
            return $"Unexpected token: {(string)NextToken!.Value!} ({NextToken.BeginLocation} - {NextToken.EndLocation})";
        }

        return $"Unexpected token: {NextToken!.Kind} ({NextToken.BeginLocation} - {NextToken.EndLocation})";
    }
}
