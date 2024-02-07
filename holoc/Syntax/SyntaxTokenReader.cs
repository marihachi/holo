using System;
using System.IO;

namespace holoc.Syntax;

public class TokenReaderSession(Stream stream)
{
    public StreamReader Reader { get; set; } = new StreamReader(stream);
    public int Column { get; set; } = 1;
    public int Line { get; set; } = 1;
}

public class SyntaxTokenReader
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

        // NOTE: SourceReader.Read()したら必ず位置の更新を行う。

        while (true)
        {
            // ストリームの終わりに達していたら
            if (Session.Reader.EndOfStream)
            {
                return ReadTokenResult.Succeed(
                    new SyntaxToken(SyntaxTokenKind.EOF, new CodeLocation(Session.Column, Session.Line))
                );
            }

            // 1文字読み取る
            var ch = (char)Session.Reader.Read();

            // スペースのスキップ
            switch (ch)
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
                    if (!Session.Reader.EndOfStream && (char)Session.Reader.Peek() == '\n')
                    {
                        Session.Reader.Read();
                        Session.Column = 1;
                        Session.Line += 1;
                        continue;
                    }
                    return ReadTokenResult.Fail("unexpected char.");
            }

            var location = new CodeLocation(Session.Column, Session.Line);
            Session.Column += 1;

            // 記号の読み取り
            switch (ch) {
                case '*':
                    if (!Session.Reader.EndOfStream && (char)Session.Reader.Peek() == '=')
                    {
                        Session.Reader.Read();
                        Session.Column += 1;
                        return ReadTokenResult.Succeed(
                            new SyntaxToken(SyntaxTokenKind.AsterEq, location)
                        );
                    }
                    else
                    {
                        return ReadTokenResult.Succeed(
                            new SyntaxToken(SyntaxTokenKind.Asterisk, location)
                        );
                    }
                case '+':
                    if (!Session.Reader.EndOfStream && (char)Session.Reader.Peek() == '=')
                    {
                        Session.Reader.Read();
                        Session.Column += 1;
                        return ReadTokenResult.Succeed(
                            new SyntaxToken(SyntaxTokenKind.PlusEq, location)
                        );
                    }
                    else
                    {
                        return ReadTokenResult.Succeed(
                            new SyntaxToken(SyntaxTokenKind.Plus, location)
                        );
                    }
                case '-':
                    if (!Session.Reader.EndOfStream && (char)Session.Reader.Peek() == '=')
                    {
                        Session.Reader.Read();
                        Session.Column += 1;
                        return ReadTokenResult.Succeed(
                            new SyntaxToken(SyntaxTokenKind.MinusEq, location)
                        );
                    }
                    else
                    {
                        return ReadTokenResult.Succeed(
                            new SyntaxToken(SyntaxTokenKind.Minus, location)
                        );
                    }
                case '/':
                    if (!Session.Reader.EndOfStream && (char)Session.Reader.Peek() == '=')
                    {
                        Session.Reader.Read();
                        Session.Column += 1;
                        return ReadTokenResult.Succeed(
                            new SyntaxToken(SyntaxTokenKind.SlashEq, location)
                        );
                    }
                    else
                    {
                        return ReadTokenResult.Succeed(
                            new SyntaxToken(SyntaxTokenKind.Slash, location)
                        );
                    }
            }

            // TODO: 識別子やキーワード、リテラルの読み取り

            return ReadTokenResult.Fail("unexpected char.");
        }
    }
}
