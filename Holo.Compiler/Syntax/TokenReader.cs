using System;
using System.IO;

namespace Holo.Compiler.Syntax;

public class TokenReader
{
    private TokenReaderSession? Session;

    public void Initialize(Stream stream)
    {
        Session = new TokenReaderSession(stream);
    }

    public ReadTokenResult Read()
    {
        if (Session == null) {
            throw new InvalidOperationException("not initialized");
        }

        // NOTE: Session.Readしたら必ず位置の更新を行う。

        while (true)
        {
            // 1文字読み取る
            Session.Read();
            
            // ストリームの終わりに達していたら
            if (Session.CurrentChar == null)
            {
                return ReadTokenResult.Succeed(
                    new SyntaxToken(TokenKind.EOF, new TokenLocation(Session.Column, Session.Line))
                );
            }

            // スペースのスキップ
            switch (Session.CurrentChar)
            {
                case ' ':
                    Session.Column += 1;
                    continue;
                case '\t':
                    Session.Column += 1;
                    continue;
                case '\n':
                    Session.Column = 1;
                    Session.Line += 1;
                    continue;
                case '\r':
                    Session.Read();
                    if (Session.CurrentChar == '\n')
                    {
                        Session.Column = 1;
                        Session.Line += 1;
                        continue;
                    }
                    return ReadTokenResult.Fail("unexpected char.");
            }

            var location = new TokenLocation(Session.Column, Session.Line);
            Session.Column += 1;

            // 記号の読み取り
            switch (Session.CurrentChar) {
                case '*':
                    if (Session.Peek() == '=')
                    {
                        Session.Read();
                        Session.Column += 1;
                        return ReadTokenResult.Succeed(
                            new SyntaxToken(TokenKind.AsterEq, location)
                        );
                    }
                    else
                    {
                        return ReadTokenResult.Succeed(
                            new SyntaxToken(TokenKind.Asterisk, location)
                        );
                    }
                case '+':
                    if (Session.Peek() == '=')
                    {
                        Session.Read();
                        Session.Column += 1;
                        return ReadTokenResult.Succeed(
                            new SyntaxToken(TokenKind.PlusEq, location)
                        );
                    }
                    else
                    {
                        return ReadTokenResult.Succeed(
                            new SyntaxToken(TokenKind.Plus, location)
                        );
                    }
                case '-':
                    if (Session.Peek() == '=')
                    {
                        Session.Read();
                        Session.Column += 1;
                        return ReadTokenResult.Succeed(
                            new SyntaxToken(TokenKind.MinusEq, location)
                        );
                    }
                    else
                    {
                        return ReadTokenResult.Succeed(
                            new SyntaxToken(TokenKind.Minus, location)
                        );
                    }
                case '/':
                    if (Session.Peek() == '=')
                    {
                        Session.Read();
                        Session.Column += 1;
                        return ReadTokenResult.Succeed(
                            new SyntaxToken(TokenKind.SlashEq, location)
                        );
                    }
                    else
                    {
                        return ReadTokenResult.Succeed(
                            new SyntaxToken(TokenKind.Slash, location)
                        );
                    }
            }

            // TODO: 識別子やキーワード、リテラルの読み取り

            return ReadTokenResult.Fail("unexpected char.");
        }
    }
}

public class TokenReaderSession(Stream stream)
{
    public StreamReader Reader { get; set; } = new StreamReader(stream);
    public char? CurrentChar { get; set; }
    public int Column { get; set; } = 1;
    public int Line { get; set; } = 1;

    public char? Read()
    {
        if (!Reader.EndOfStream)
        {
            CurrentChar = (char)Reader.Read();
        }
        else
        {
            CurrentChar = null;
        }

        return CurrentChar;
    }

    public char? Peek()
    {
        if (!Reader.EndOfStream)
        {
            return (char)Reader.Peek();
        }
        else
        {
            return null;
        }
    }
}

public class ReadTokenResult(bool isSuccess, SyntaxToken? token, string? message)
{
    public bool IsSuccess { get; set; } = isSuccess;
    public SyntaxToken? Token { get; set; } = token;
    public string? Message { get; set; } = message;

    public static ReadTokenResult Succeed(SyntaxToken token)
    {
        return new ReadTokenResult(true, token, null);
    }

    public static ReadTokenResult Fail(string message)
    {
        return new ReadTokenResult(false, null, message);
    }
}
