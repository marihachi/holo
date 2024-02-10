using System;
using System.IO;

namespace Holo.Compiler.Syntax;

public class SyntaxTokenReader
{
    private ReaderSession? Session;

    public void Initialize(Stream stream)
    {
        Session = new ReaderSession(stream);
    }

    public ReadResult Read()
    {
        if (Session == null) {
            throw new InvalidOperationException("not initialized");
        }

        // NOTE: Session.Reader.Readしたら必ず位置の更新を行う。

        while (true)
        {
            // ストリームの終わりに達していたら
            if (Session.Reader.EndOfStream)
            {
                return ReadResult.Succeed(
                    new SyntaxToken(TokenKind.EOF, new TokenLocation(Session.Column, Session.Line))
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
                    return ReadResult.Fail("unexpected char.");
            }

            var location = new TokenLocation(Session.Column, Session.Line);
            Session.Column += 1;

            // 記号の読み取り
            switch (ch) {
                case '*':
                    if (!Session.Reader.EndOfStream && (char)Session.Reader.Peek() == '=')
                    {
                        Session.Reader.Read();
                        Session.Column += 1;
                        return ReadResult.Succeed(
                            new SyntaxToken(TokenKind.AsterEq, location)
                        );
                    }
                    else
                    {
                        return ReadResult.Succeed(
                            new SyntaxToken(TokenKind.Asterisk, location)
                        );
                    }
                case '+':
                    if (!Session.Reader.EndOfStream && (char)Session.Reader.Peek() == '=')
                    {
                        Session.Reader.Read();
                        Session.Column += 1;
                        return ReadResult.Succeed(
                            new SyntaxToken(TokenKind.PlusEq, location)
                        );
                    }
                    else
                    {
                        return ReadResult.Succeed(
                            new SyntaxToken(TokenKind.Plus, location)
                        );
                    }
                case '-':
                    if (!Session.Reader.EndOfStream && (char)Session.Reader.Peek() == '=')
                    {
                        Session.Reader.Read();
                        Session.Column += 1;
                        return ReadResult.Succeed(
                            new SyntaxToken(TokenKind.MinusEq, location)
                        );
                    }
                    else
                    {
                        return ReadResult.Succeed(
                            new SyntaxToken(TokenKind.Minus, location)
                        );
                    }
                case '/':
                    if (!Session.Reader.EndOfStream && (char)Session.Reader.Peek() == '=')
                    {
                        Session.Reader.Read();
                        Session.Column += 1;
                        return ReadResult.Succeed(
                            new SyntaxToken(TokenKind.SlashEq, location)
                        );
                    }
                    else
                    {
                        return ReadResult.Succeed(
                            new SyntaxToken(TokenKind.Slash, location)
                        );
                    }
            }

            // TODO: 識別子やキーワード、リテラルの読み取り

            return ReadResult.Fail("unexpected char.");
        }
    }

    public class ReaderSession(Stream stream)
    {
        public StreamReader Reader { get; set; } = new StreamReader(stream);
        public int Column { get; set; } = 1;
        public int Line { get; set; } = 1;
    }

    public class ReadResult(bool isSuccess, SyntaxToken? token, string? message)
    {
        public bool IsSuccess { get; set; } = isSuccess;
        public SyntaxToken? Token { get; set; } = token;
        public string? Message { get; set; } = message;

        public static ReadResult Succeed(SyntaxToken token)
        {
            return new ReadResult(true, token, null);
        }

        public static ReadResult Fail(string message)
        {
            return new ReadResult(false, null, message);
        }
    }
}
