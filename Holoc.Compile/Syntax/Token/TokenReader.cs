namespace Holoc.Compile.Syntax.Token;

public class TokenReader
{
    private StreamReader? Stream;

    public TokenLocation CurrentLocation { get; private set; }

    public SyntaxToken? CurrentToken { get; private set; }

    private char? NextChar;

    public TokenLocation NextLocation { get; private set; }

    // NextTokenはSetNextTokenから設定する。CurrentTokenへの反映が必要なため。
    public SyntaxToken? NextToken { get; private set; }
    
    public string? Error { get; private set; }

    public void Initialize(StreamReader stream)
    {
        Stream = stream;
        CurrentLocation = new(0, 0);
        CurrentToken = null;
        NextChar = null;
        NextLocation = new(0, 0);
        NextToken = null;
        Error = null;

        // 初回読み取り
        ReadNextCharInternal(true);
    }

    private bool ReadNextChar()
    {
        //Console.WriteLine("[ReadNextChar]");
        ReadNextCharInternal(false);
        return NextChar != null;
    }

    private void ReadNextCharInternal(bool firstRead)
    {
        if (Stream!.EndOfStream)
        {
            SetNextChar(null);
            return;
        }

        if (firstRead)
        {
            NextLocation = new(1, 0);
        }
        else
        {
            CurrentLocation = NextLocation;
            //Console.WriteLine($"CurrentLocation = {CurrentLocation}");
        }

        char? ch;
        while (true)
        {
            int data = Stream.Read();

            if (data == -1)
            {
                ch = null;
            }
            else
            {
                ch = (char)data;
            }

            if (ch == '\r')
            {
                continue;
            }

            if (ch == '\n')
            {
                MoveNewLine();
                continue;
            }

            MoveRight();
            break;
        }

        SetNextChar(ch);

        if (firstRead)
        {
            CurrentLocation = new(1, 1);
            //Console.WriteLine($"CurrentLocation = {CurrentLocation}");
        }
    }

    private void SetNextChar(char? ch)
    {
        //Console.WriteLine($"NextChar = '{ch}'");

        NextChar = ch;
    }

    /// <summary>
    /// 読み取り位置を次の文字に進めます。
    /// </summary>
    private void MoveRight()
    {
        NextLocation = NextLocation.MoveRight();
        //Console.WriteLine($"NextLocation = {NextLocation}");
    }

    /// <summary>
    /// 読み取り位置を次の行の先頭に進めます。
    /// </summary>
    private void MoveNewLine()
    {
        NextLocation = NextLocation.MoveNewLine();
        //Console.WriteLine($"NextLocation = {NextLocation}");
    }

    private void SetNextToken(SyntaxToken token)
    {
        CurrentToken = NextToken;
        NextToken = token;
        //Console.WriteLine($"[{token.Kind}] {token.BeginLocation} - {token.EndLocation}");
    }

    private TokenLocation GetNextLocation()
    {
        return NextLocation;
    }

    private TokenLocation GetCurrentLocation()
    {
        return CurrentLocation;
    }

    /// <summary>
    /// トークンを読み取ります。
    /// 読み取りに失敗した場合は、Errorプロパティにエラーの内容が設定されます。
    /// </summary>
    public bool Read()
    {
        //Console.WriteLine("[Read]");

        ReadInternal();
        return Error == null;
    }

    private void ReadInternal()
    {
        TokenLocation beginLocation;

        if (Stream == null)
        {
            throw new InvalidOperationException("not initialized");
        }

        while (true)
        {
            // ストリームの終わりに達していたら
            if (NextChar == null)
            {
                SetNextToken(new SyntaxToken(TokenKind.EOF, GetNextLocation(), GetNextLocation()));
                return;
            }

            switch (NextChar)
            {
                case ' ':
                    ReadNextChar();
                    continue;

                case '\t':
                    ReadNextChar();
                    continue;

                case '{':
                    beginLocation = GetNextLocation();
                    ReadNextChar();
                    SetNextToken(new SyntaxToken(TokenKind.OpenBrace, beginLocation, GetCurrentLocation().MoveRight()));
                    return;

                case '}':
                    beginLocation = GetNextLocation();
                    ReadNextChar();
                    SetNextToken(new SyntaxToken(TokenKind.CloseBrace, beginLocation, GetCurrentLocation().MoveRight()));
                    return;

                case '(':
                    beginLocation = GetNextLocation();
                    ReadNextChar();
                    SetNextToken(new SyntaxToken(TokenKind.OpenParen, beginLocation, GetCurrentLocation().MoveRight()));
                    return;

                case ')':
                    beginLocation = GetNextLocation();
                    ReadNextChar();
                    SetNextToken(new SyntaxToken(TokenKind.CloseParen, beginLocation, GetCurrentLocation().MoveRight()));
                    return;

                case ',':
                    beginLocation = GetNextLocation();
                    ReadNextChar();
                    SetNextToken(new SyntaxToken(TokenKind.Comma, beginLocation, GetCurrentLocation().MoveRight()));
                    return;

                case '=':
                    beginLocation = GetNextLocation();
                    ReadNextChar();
                    SetNextToken(new SyntaxToken(TokenKind.Eq, beginLocation, GetCurrentLocation().MoveRight()));
                    return;

                case ':':
                    beginLocation = GetNextLocation();
                    ReadNextChar();
                    SetNextToken(new SyntaxToken(TokenKind.Colon, beginLocation, GetCurrentLocation().MoveRight()));
                    return;

                case ';':
                    beginLocation = GetNextLocation();
                    ReadNextChar();
                    SetNextToken(new SyntaxToken(TokenKind.SemiColon, beginLocation, GetCurrentLocation().MoveRight()));
                    return;

                case '*':
                    beginLocation = GetNextLocation();
                    ReadNextChar();

                    // 先読み
                    if (NextChar == '=')
                    {
                        ReadNextChar();
                        SetNextToken(new SyntaxToken(TokenKind.AsterEq, beginLocation, GetCurrentLocation().MoveRight()));
                        return;
                    }

                    SetNextToken(new SyntaxToken(TokenKind.Asterisk, beginLocation, GetCurrentLocation().MoveRight()));
                    return;

                case '+':
                    beginLocation = GetNextLocation();
                    ReadNextChar();

                    // 先読み
                    if (NextChar == '=')
                    {
                        ReadNextChar();
                        SetNextToken(new SyntaxToken(TokenKind.PlusEq, beginLocation, GetCurrentLocation().MoveRight()));
                        return;
                    }

                    SetNextToken(new SyntaxToken(TokenKind.Plus, beginLocation, GetCurrentLocation().MoveRight()));
                    return;

                case '-':
                    beginLocation = GetNextLocation();
                    ReadNextChar();

                    // 先読み
                    if (NextChar == '=')
                    {
                        ReadNextChar();
                        SetNextToken(new SyntaxToken(TokenKind.MinusEq, beginLocation, GetCurrentLocation().MoveRight()));
                        return;
                    }

                    SetNextToken(new SyntaxToken(TokenKind.Minus, beginLocation, GetCurrentLocation().MoveRight()));
                    return;

                case '/':
                    beginLocation = GetNextLocation();
                    ReadNextChar();

                    // 先読み
                    if (NextChar == '=')
                    {
                        ReadNextChar();
                        SetNextToken(new SyntaxToken(TokenKind.SlashEq, beginLocation, GetCurrentLocation().MoveRight()));
                        return;
                    }

                    SetNextToken(new SyntaxToken(TokenKind.Slash, beginLocation, GetCurrentLocation().MoveRight()));
                    return;
            }

            // 数字
            if (NextChar >= '0' && NextChar <= '9')
            {
                beginLocation = GetNextLocation();
                string wholeNumber = "";

                wholeNumber += NextChar;
                ReadNextChar();

                // 後続の文字を読む
                while (true)
                {
                    var ch = NextChar;

                    if (ch == null || !(ch >= '0' && ch <= '9'))
                    {
                        break;
                    }

                    wholeNumber += ch;
                    ReadNextChar();
                }

                int value = int.Parse(wholeNumber);
                SetNextToken(new SyntaxToken(TokenKind.NumberLiteral, beginLocation, GetCurrentLocation().MoveRight(), value));
                return;
            }

            // 識別子またはキーワード
            if (NextChar >= 'A' && NextChar <= 'Z' ||
                NextChar >= 'a' && NextChar <= 'z' ||
                NextChar == '_')
            {
                beginLocation = GetNextLocation();
                string value = "";

                value += NextChar;
                ReadNextChar();

                // 後続の文字を読む
                while (true)
                {
                    var ch = NextChar;

                    if (ch == null || !(
                        ch >= '0' && ch <= '9' ||
                        ch >= 'A' && ch <= 'Z' ||
                        ch >= 'a' && ch <= 'z' ||
                        ch == '_'))
                    {
                        break;
                    }

                    value += ch;
                    ReadNextChar();
                }

                SetNextToken(new SyntaxToken(TokenKind.Word, beginLocation, GetCurrentLocation().MoveRight(), value));
                return;
            }

            Error = $"Unexpected char: '{NextChar}' ({NextLocation})";
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
