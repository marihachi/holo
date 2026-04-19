namespace Holoc.Compile.Syntax.Token;

public class TokenReader
{
    private StreamReader? Stream;

    private char? CurrentChar;

    public int Column { get; private set; }

    public int Line { get; private set; }

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

    /// <summary>
    /// 読み取り位置を次の文字に進めます。
    /// </summary>
    private void NextChar()
    {
        Column += 1;
        //Console.WriteLine($"{Line}:{Column}");
    }

    /// <summary>
    /// 読み取り位置を次の行の先頭に進めます。
    /// </summary>
    private void NextNewLine()
    {
        Column = 1;
        Line += 1;
        //Console.WriteLine($"{Line}:{Column}");
    }

    private void SetNextToken(SyntaxToken token)
    {
        CurrentToken = NextToken;
        NextToken = token;
    }

    private TokenLocation GetLocation()
    {
        return new TokenLocation(Column, Line);
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
                SetNextToken(new SyntaxToken(TokenKind.EOF, GetLocation(), GetLocation()));
                return;
            }

            switch (CurrentChar)
            {
                case ' ':
                    NextChar();
                    continue;

                case '\t':
                    NextChar();
                    continue;

                // LF
                case '\n':
                    NextNewLine();
                    continue;

                // CR
                case '\r':
                    // LFが続いていたら一緒に消費する
                    if (PeekChar() == '\n')
                    {
                        ReadChar();
                    }
                    NextNewLine();
                    continue;

                case '{':
                    beginLocation = GetLocation();
                    NextChar();
                    SetNextToken(new SyntaxToken(TokenKind.OpenBrace, beginLocation, GetLocation()));
                    return;

                case '}':
                    beginLocation = GetLocation();
                    NextChar();
                    SetNextToken(new SyntaxToken(TokenKind.CloseBrace, beginLocation, GetLocation()));
                    return;

                case '(':
                    beginLocation = GetLocation();
                    NextChar();
                    SetNextToken(new SyntaxToken(TokenKind.OpenParen, beginLocation, GetLocation()));
                    return;

                case ')':
                    beginLocation = GetLocation();
                    NextChar();
                    SetNextToken(new SyntaxToken(TokenKind.CloseParen, beginLocation, GetLocation()));
                    return;

                case ',':
                    beginLocation = GetLocation();
                    NextChar();
                    SetNextToken(new SyntaxToken(TokenKind.Comma, beginLocation, GetLocation()));
                    return;

                case '=':
                    beginLocation = GetLocation();
                    NextChar();
                    SetNextToken(new SyntaxToken(TokenKind.Eq, beginLocation, GetLocation()));
                    return;

                case ':':
                    beginLocation = GetLocation();
                    NextChar();
                    SetNextToken(new SyntaxToken(TokenKind.Colon, beginLocation, GetLocation()));
                    return;

                case ';':
                    beginLocation = GetLocation();
                    NextChar();
                    SetNextToken(new SyntaxToken(TokenKind.SemiColon, beginLocation, GetLocation()));
                    return;

                case '*':
                    beginLocation = GetLocation();
                    NextChar();

                    // 先読み
                    if (PeekChar() == '=')
                    {
                        ReadChar();
                        NextChar();
                        SetNextToken(new SyntaxToken(TokenKind.AsterEq, beginLocation, GetLocation()));
                        return;
                    }

                    SetNextToken(new SyntaxToken(TokenKind.Asterisk, beginLocation, GetLocation()));
                    return;

                case '+':
                    beginLocation = GetLocation();
                    NextChar();

                    // 先読み
                    if (PeekChar() == '=')
                    {
                        ReadChar();
                        NextChar();
                        SetNextToken(new SyntaxToken(TokenKind.PlusEq, beginLocation, GetLocation()));
                        return;
                    }

                    SetNextToken(new SyntaxToken(TokenKind.Plus, beginLocation, GetLocation()));
                    return;

                case '-':
                    beginLocation = GetLocation();
                    NextChar();

                    // 先読み
                    if (PeekChar() == '=')
                    {
                        ReadChar();
                        NextChar();
                        SetNextToken(new SyntaxToken(TokenKind.MinusEq, beginLocation, GetLocation()));
                        return;
                    }

                    SetNextToken(new SyntaxToken(TokenKind.Minus, beginLocation, GetLocation()));
                    return;

                case '/':
                    beginLocation = GetLocation();
                    NextChar();

                    // 先読み
                    if (PeekChar() == '=')
                    {
                        ReadChar();
                        NextChar();
                        SetNextToken(new SyntaxToken(TokenKind.SlashEq, beginLocation, GetLocation()));
                        return;
                    }

                    SetNextToken(new SyntaxToken(TokenKind.Slash, beginLocation, GetLocation()));
                    return;
            }

            // 数字
            if (CurrentChar >= '0' && CurrentChar <= '9')
            {
                beginLocation = GetLocation();
                string wholeNumber = "";

                wholeNumber += CurrentChar;
                NextChar();

                // 後続の文字を読む
                while (true)
                {
                    var ch = PeekChar();
                    if (ch == null || !(ch >= '0' && ch <= '9')) break;

                    ReadChar();
                    wholeNumber += ch;
                    NextChar();
                }

                int value = int.Parse(wholeNumber);
                SetNextToken(new SyntaxToken(TokenKind.NumberLiteral, beginLocation, GetLocation(), value));
                return;
            }

            // 識別子またはキーワード
            if (CurrentChar >= 'A' && CurrentChar <= 'Z' ||
                CurrentChar >= 'a' && CurrentChar <= 'z' ||
                CurrentChar == '_')
            {
                beginLocation = GetLocation();
                string value = "";

                value += CurrentChar;
                NextChar();

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
                    NextChar();
                }

                SetNextToken(new SyntaxToken(TokenKind.Word, beginLocation, GetLocation(), value));
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
