using System;
using System.IO;

namespace Holo.Compiler.Syntax;

public class ParserContext
{
    private TokenReader Reader = new TokenReader();

    public ReadTokenResult? Result;
    public bool? IsSuccess => Result?.IsSuccess;
    public SyntaxToken? Token => Result?.Token;
    public TokenKind? Kind => Token?.Kind;
    public string? Message => Result?.Message;

    public void Initialize(Stream stream)
    {
        Reader.Initialize(stream);
    }

    private Exception CreateUnexpectedError()
    {
        return new Exception($"unexpected token: {Kind}");
    }

    public bool Read()
    {
        Result = Reader.Read();
        return Result.IsSuccess;
    }

    public void Peek()
    {
        throw new NotImplementedException();
    }

    public void Expect(TokenKind kind)
    {
        if (Kind != kind)
        {
            throw CreateUnexpectedError();
        }
    }

    public void ReadAs(TokenKind kind)
    {
        Expect(kind);
        Read();
    }
}
