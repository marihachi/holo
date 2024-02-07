using System.IO;

namespace holoc.Syntax;

public class SyntaxParser
{
    public void Parse(Stream stream)
    {
        var ctx = new SyntaxParserContext();
        ctx.Initialize(stream);
    }
}
