using System;

namespace holoc.Syntax;

public class SyntaxParserContext(SyntaxTokenReader reader)
{
    private SyntaxTokenReader Reader = reader;

    public SyntaxToken? Token { get; set; }
    public SyntaxTokenKind? Kind => Token?.Kind;
    public string? ErrorMessage { get; set; }

    private Exception CreateUnexpectedError()
    {
        return new Exception($"unexpected token: {Kind}");
    }

    public bool Read()
    {
        Token = null;
        ErrorMessage = null;

        var result = Reader.Read();

        Token = result.Token;
        ErrorMessage = result.Message;

        return result.IsSuccess;
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
