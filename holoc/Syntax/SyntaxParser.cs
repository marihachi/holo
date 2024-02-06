using System;
using System.IO;
using System.Text;

namespace holoc.Syntax;

public class SyntaxParser
{
    public void Parse(string input)
    {
        var stream = new MemoryStream(1024);

        stream.Write(Encoding.UTF8.GetBytes("+"));

        var tokenReader = new SyntaxTokenReader(stream);
        var ctx = new SyntaxParserContext(tokenReader);

        Console.WriteLine("result: {0}", ctx.Read());
        Console.WriteLine("token: {0}", ctx.Token);
        Console.WriteLine("kind: {0}", ctx.Kind);
        Console.WriteLine("error: {0}", ctx.ErrorMessage);

        stream.Write(Encoding.UTF8.GetBytes("*"));

        Console.WriteLine("result: {0}", ctx.Read());
        Console.WriteLine("token: {0}", ctx.Token);
        Console.WriteLine("kind: {0}", ctx.Kind);
        Console.WriteLine("error: {0}", ctx.ErrorMessage);

        Console.WriteLine("result: {0}", ctx.Read());
        Console.WriteLine("token: {0}", ctx.Token);
        Console.WriteLine("kind: {0}", ctx.Kind);
        Console.WriteLine("error: {0}", ctx.ErrorMessage);
    }
}
