namespace Holoc.Compile.Syntax.Token;

public class TokenReader
{
    private StreamReader? Stream;

    private char? CurrentChar;

    public int PrevLine { get; private set; }

    public int PrevColumn { get; private set; }

    public int Line { get; private set; }

    public int Column { get; private set; }

    public SyntaxToken? CurrentToken { get; private set; }

    // NextTokenはSetNextTokenから設定する。CurrentTokenへの反映が必要なため。
    public SyntaxToken? NextToken { get; private set; }
    
    public string? Error { get; private set; }

    public void Initialize(StreamReader stream)
    {
        Stream = stream;
        CurrentChar = null;
        PrevLine = 1;
        PrevColumn = 1;
        Line = 1;
        Column = 1;
        CurrentToken = null;
        NextToken = null;
        Error = null;

        // 最初の文字を読み取る
        ReadChar();
    }

    private bool ReadChar()
    {
        ReadCharInternal();
        return CurrentChar != null;
    }

    private void ReadCharInternal()
    {
        if (Stream!.EndOfStream)
        {
            SetCurrentChar(null);
            return;
        }

        var ch = (char)Stream.Read();

        if (ch == '\r')
        {
            // 位置に影響を与えないため無視
        }
        else if (ch == '\n')
        {
            MoveNewLine();
        }
        else
        {
            MoveRight();
        }

        if (ch == '\0')
        {
            SetCurrentChar(null);
            return;
        }

        SetCurrentChar(ch);
    }

    private void SetCurrentChar(char? ch)
    {
        CurrentChar = ch;
    }

    /// <summary>
    /// 読み取り位置を次の文字に進めます。
    /// </summary>
    private void MoveRight()
    {
        PrevLine = Line;
        PrevColumn = Column;

        Column += 1;
        //Console.WriteLine($"{Line}:{Column}");
    }

    /// <summary>
    /// 読み取り位置を次の行の先頭に進めます。
    /// </summary>
    private void MoveNewLine()
    {
        PrevLine = Line;
        PrevColumn = Column;

        Column = 1;
        Line += 1;
        //Console.WriteLine($"{Line}:{Column}");
    }

    private char? GetChar()
    {
        return CurrentChar;
    }

    private void SetNextToken(SyntaxToken token)
    {
        CurrentToken = NextToken;
        NextToken = token;
    }

    private TokenLocation GetLocation()
    {
        return new TokenLocation(PrevColumn, PrevLine);
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

        TokenLocation beginLocation;

        if (Stream == null)
        {
            throw new InvalidOperationException("not initialized");
        }

        while (true)
        {
            // ストリームの終わりに達していたら
            if (GetChar() == null)
            {
                SetNextToken(new SyntaxToken(TokenKind.EOF, GetLocation(), GetLocation()));
                return;
            }

            switch (GetChar())
            {
                case ' ':
                    ReadChar();
                    continue;

                case '\t':
                    ReadChar();
                    continue;

                // LF
                case '\n':
                    ReadChar();
                    continue;

                // CR
                case '\r':
                    ReadChar();

                    // LFが続いていたら一緒に消費する
                    if (GetChar() == '\n')
                    {
                        ReadChar();
                    }
                    continue;

                case '{':
                    beginLocation = GetLocation();
                    ReadChar();
                    SetNextToken(new SyntaxToken(TokenKind.OpenBrace, beginLocation, GetLocation()));
                    return;

                case '}':
                    beginLocation = GetLocation();
                    ReadChar();
                    SetNextToken(new SyntaxToken(TokenKind.CloseBrace, beginLocation, GetLocation()));
                    return;

                case '(':
                    beginLocation = GetLocation();
                    ReadChar();
                    SetNextToken(new SyntaxToken(TokenKind.OpenParen, beginLocation, GetLocation()));
                    return;

                case ')':
                    beginLocation = GetLocation();
                    ReadChar();
                    SetNextToken(new SyntaxToken(TokenKind.CloseParen, beginLocation, GetLocation()));
                    return;

                case ',':
                    beginLocation = GetLocation();
                    ReadChar();
                    SetNextToken(new SyntaxToken(TokenKind.Comma, beginLocation, GetLocation()));
                    return;

                case '=':
                    beginLocation = GetLocation();
                    ReadChar();
                    SetNextToken(new SyntaxToken(TokenKind.Eq, beginLocation, GetLocation()));
                    return;

                case ':':
                    beginLocation = GetLocation();
                    ReadChar();
                    SetNextToken(new SyntaxToken(TokenKind.Colon, beginLocation, GetLocation()));
                    return;

                case ';':
                    beginLocation = GetLocation();
                    ReadChar();
                    SetNextToken(new SyntaxToken(TokenKind.SemiColon, beginLocation, GetLocation()));
                    return;

                case '*':
                    beginLocation = GetLocation();
                    ReadChar();

                    // 先読み
                    if (GetChar() == '=')
                    {
                        ReadChar();
                        SetNextToken(new SyntaxToken(TokenKind.AsterEq, beginLocation, GetLocation()));
                        return;
                    }

                    SetNextToken(new SyntaxToken(TokenKind.Asterisk, beginLocation, GetLocation()));
                    return;

                case '+':
                    beginLocation = GetLocation();
                    ReadChar();

                    // 先読み
                    if (GetChar() == '=')
                    {
                        ReadChar();
                        SetNextToken(new SyntaxToken(TokenKind.PlusEq, beginLocation, GetLocation()));
                        return;
                    }

                    SetNextToken(new SyntaxToken(TokenKind.Plus, beginLocation, GetLocation()));
                    return;

                case '-':
                    beginLocation = GetLocation();
                    ReadChar();

                    // 先読み
                    if (GetChar() == '=')
                    {
                        ReadChar();
                        SetNextToken(new SyntaxToken(TokenKind.MinusEq, beginLocation, GetLocation()));
                        return;
                    }

                    SetNextToken(new SyntaxToken(TokenKind.Minus, beginLocation, GetLocation()));
                    return;

                case '/':
                    beginLocation = GetLocation();
                    ReadChar();

                    // 先読み
                    if (GetChar() == '=')
                    {
                        ReadChar();
                        SetNextToken(new SyntaxToken(TokenKind.SlashEq, beginLocation, GetLocation()));
                        return;
                    }

                    SetNextToken(new SyntaxToken(TokenKind.Slash, beginLocation, GetLocation()));
                    return;
            }

            // 数字
            if (GetChar() >= '0' && GetChar() <= '9')
            {
                beginLocation = GetLocation();
                string wholeNumber = "";

                wholeNumber += GetChar();
                ReadChar();

                // 後続の文字を読む
                while (true)
                {
                    var ch = GetChar();

                    if (ch == null || !(ch >= '0' && ch <= '9'))
                    {
                        break;
                    }

                    wholeNumber += ch;
                    ReadChar();
                }

                int value = int.Parse(wholeNumber);
                SetNextToken(new SyntaxToken(TokenKind.NumberLiteral, beginLocation, GetLocation(), value));
                return;
            }

            // 識別子またはキーワード
            if (GetChar() >= 'A' && GetChar() <= 'Z' ||
                GetChar() >= 'a' && GetChar() <= 'z' ||
                GetChar() == '_')
            {
                beginLocation = GetLocation();
                string value = "";

                value += GetChar();
                ReadChar();

                // 後続の文字を読む
                while (true)
                {
                    var ch = GetChar();

                    if (ch == null || !(
                        ch >= '0' && ch <= '9' ||
                        ch >= 'A' && ch <= 'Z' ||
                        ch >= 'a' && ch <= 'z' ||
                        ch == '_'))
                    {
                        break;
                    }

                    value += ch;
                    ReadChar();
                }

                SetNextToken(new SyntaxToken(TokenKind.Word, beginLocation, GetLocation(), value));
                return;
            }

            Error = $"Unexpected char: '{GetChar()}' ({Line}:{Column})";
            return;
        }
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
