using System;
using System.IO;

namespace Holo.Compiler.Syntax;

public class TokenReader
{
    private StreamReader? Stream;
    private char? CurrentChar;
    private SyntaxToken? _Token;
    private string? _Message;

    public int Column { get; private set; }
    public int Line { get; private set; }
    public bool IsSuccess => Token != null;
    public SyntaxToken Token => _Token!;
    public TokenKind TokenKind => Token.Kind;
    public string Message => _Message!;

    private void ReadChar()
    {
        if (Stream == null) {
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
        if (Stream == null) {
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

    private void SetResult(SyntaxToken token)
    {
        _Token = token;
    }

    private void SetError(string message)
    {
        _Message = message;
    }

    public void Initialize(Stream stream)
    {
        Stream = new StreamReader(stream);
        CurrentChar = null;
        Column = 1;
        Line = 1;
        _Token = null;
        _Message = null;
    }

    public string CreateUnexpectedError()
    {
        return $"unexpected token: {Token.Kind}";
    }

    public bool Read()
    {
        ReadInternal();
        return IsSuccess;
    }

    private void ReadInternal()
    {
        if (Stream == null) {
            throw new InvalidOperationException("not initialized");
        }

        // NOTE: ReadCharしたら必ず位置の更新を行う。

        while (true)
        {
            // 1文字読み取る
            ReadChar();

            // ストリームの終わりに達していたら
            if (CurrentChar == null)
            {
                SetResult(new SyntaxToken(TokenKind.EOF, new TokenLocation(Column, Line)));
                return;
            }

            // スペースのスキップ
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
                    SetError("unexpected char.");
                    return;
            }

            var location = new TokenLocation(Column, Line);
            Column += 1;

            // 記号の読み取り
            switch (CurrentChar) {
                case '*':
                    if (PeekChar() == '=')
                    {
                        ReadChar();
                        Column += 1;
                        SetResult(new SyntaxToken(TokenKind.AsterEq, location));
                    }
                    else
                    {
                        SetResult(new SyntaxToken(TokenKind.Asterisk, location));
                    }
                    return;

                case '+':
                    if (PeekChar() == '=')
                    {
                        ReadChar();
                        Column += 1;
                        SetResult(new SyntaxToken(TokenKind.PlusEq, location));
                    }
                    else
                    {
                        SetResult(new SyntaxToken(TokenKind.Plus, location));
                    }
                    return;

                case '-':
                    if (PeekChar() == '=')
                    {
                        ReadChar();
                        Column += 1;
                        SetResult(new SyntaxToken(TokenKind.MinusEq, location));
                    }
                    else
                    {
                        SetResult(new SyntaxToken(TokenKind.Minus, location));
                    }
                    return;

                case '/':
                    if (PeekChar() == '=')
                    {
                        ReadChar();
                        Column += 1;
                        SetResult(new SyntaxToken(TokenKind.SlashEq, location));
                    }
                    else
                    {
                        SetResult(new SyntaxToken(TokenKind.Slash, location));
                    }
                    return;
            }

            // TODO: 識別子やキーワード、リテラルの読み取り

            SetError("unexpected char.");
            return;
        }
    }

    public void Peek()
    {
        throw new NotImplementedException();
    }
}
