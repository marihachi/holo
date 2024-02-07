using System;
using System.CommandLine;
using System.IO.MemoryMappedFiles;
using holoc.Syntax;

public class Program
{
    static int Main(string[] args)
    {
        var rootCommand = new RootCommand("The holo compiler");

        var inputArg = new Argument<string[]>("input", "list of input files.");
        rootCommand.Add(inputArg);

        rootCommand.SetHandler((ctx) =>
        {
            var inputArgValue = ctx.ParseResult.GetValueForArgument(inputArg);
            // do something

            Console.WriteLine($"input files: {inputArgValue.Length}");

            using var mmf = MemoryMappedFile.CreateFromFile("debug/main.ho");
            using var stream = mmf.CreateViewStream();

            var parser = new SyntaxParser();
            parser.Parse(stream);
        });

        return rootCommand.InvokeAsync(args).Result;
    }
}
