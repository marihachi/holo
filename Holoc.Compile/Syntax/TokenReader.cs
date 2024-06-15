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
        if (Stream == null)
        {
            throw new InvalidOperationException("not initialized");
        }

        if (!Stream.EndOfStream)
        {
            CurrentChar = (char)Stream.Read();
        }
        else
        {
            CurrentChar = null;
        }
    }

    private char? PeekChar()
    {
        if (Stream == null)
        {
            throw new InvalidOperationException("not initialized");
        }

        if (!Stream.EndOfStream)
        {
            return (char)Stream.Peek();
        }
        else
        {
            return null;
        }
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
        return $"unexpected token: {Token!.Kind}";
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
        int state = 0;

        while (true)
        {
            // 1文字読み取る
            ReadChar();

            // ストリームの終わりに達していたら
            if (CurrentChar == null)
            {
                Token = new SyntaxToken(TokenKind.EOF, new TokenLocation(Column, Line));
                return;
            }

            TokenLocation beginLocation;

            switch (state)
            {
                // トークン読み取り状態
                case 0:
                    switch (CurrentChar)
                    {
                        case ' ':
                            Column += 1;
                            continue;

                        case '\t':
                            Column += 1;
                            continue;

                        case '\n':
                            Column = 1;
                            Line += 1;
                            continue;

                        case '\r':
                            ReadChar();
                            if (CurrentChar == '\n')
                            {
                                Column = 1;
                                Line += 1;
                                continue;
                            }
                            Error = "unexpected char.";
                            return;

                        case '*':
                            beginLocation = new TokenLocation(Column, Line);
                            Column += 1;

                            if (PeekChar() == '=')
                            {
                                ReadChar();
                                Column += 1;
                                Token = new SyntaxToken(TokenKind.AsterEq, beginLocation);
                            }
                            else
                            {
                                Token = new SyntaxToken(TokenKind.Asterisk, beginLocation);
                            }
                            return;

                        case '+':
                            beginLocation = new TokenLocation(Column, Line);
                            Column += 1;

                            if (PeekChar() == '=')
                            {
                                ReadChar();
                                Column += 1;
                                Token = new SyntaxToken(TokenKind.PlusEq, beginLocation);
                            }
                            else
                            {
                                Token = new SyntaxToken(TokenKind.Plus, beginLocation);
                            }
                            return;

                        case '-':
                            beginLocation = new TokenLocation(Column, Line);
                            Column += 1;

                            if (PeekChar() == '=')
                            {
                                ReadChar();
                                Column += 1;
                                Token = new SyntaxToken(TokenKind.MinusEq, beginLocation);
                            }
                            else
                            {
                                Token = new SyntaxToken(TokenKind.Minus, beginLocation);
                            }
                            return;

                        case '/':
                            beginLocation = new TokenLocation(Column, Line);
                            Column += 1;

                            if (PeekChar() == '=')
                            {
                                ReadChar();
                                Column += 1;
                                Token = new SyntaxToken(TokenKind.SlashEq, beginLocation);
                            }
                            else
                            {
                                Token = new SyntaxToken(TokenKind.Slash, beginLocation);
                            }
                            return;
                    }
                    break;

                // 識別子またはキーワード
                case 1:
                    break;

                // 数値リテラル
                case 2:
                    break;

                // 文字列リテラル
                case 3:
                    break;
            }

            Error = "unexpected char.";
            return;
        }
    }

    public void Peek()
    {
        throw new NotImplementedException();
    }
}
