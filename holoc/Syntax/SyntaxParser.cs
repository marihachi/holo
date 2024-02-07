using System;
using System.IO.MemoryMappedFiles;

namespace holoc.Syntax;

public class SyntaxParser
{
    public void Parse(string input)
    {
        using var mmf = MemoryMappedFile.CreateFromFile("debug/main.ho");
        using var stream = mmf.CreateViewStream();

        var ctx = new SyntaxParserContext();
        ctx.Initialize(stream);

        ctx.Read();
        Console.WriteLine("IsSuccess: {0}", ctx.IsSuccess);
        Console.WriteLine("Kind: {0}", ctx.Kind);
        Console.WriteLine("Message: {0}", ctx.Message);

        ctx.Read();
        Console.WriteLine("IsSuccess: {0}", ctx.IsSuccess);
        Console.WriteLine("Kind: {0}", ctx.Kind);
        Console.WriteLine("Message: {0}", ctx.Message);

        ctx.Read();
        Console.WriteLine("IsSuccess: {0}", ctx.IsSuccess);
        Console.WriteLine("Kind: {0}", ctx.Kind);
        Console.WriteLine("Message: {0}", ctx.Message);
    }
}
