using System;
using System.IO;

namespace holoc.Syntax;

public class SyntaxParserContext
{
    private SyntaxTokenReader Reader = new SyntaxTokenReader();

    public ReadTokenResult? Result;
    public bool? IsSuccess => Result?.IsSuccess;
    public SyntaxToken? Token => Result?.Token;
    public SyntaxTokenKind? Kind => Token?.Kind;
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

    public void Expect(SyntaxTokenKind kind)
    {
        if (Kind != kind)
        {
            throw CreateUnexpectedError();
        }
    }

    public void ReadAs(SyntaxTokenKind kind)
    {
        Expect(kind);
        Read();
    }
}
