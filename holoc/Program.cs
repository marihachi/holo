using System;
using System.CommandLine;
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

            var parser = new SyntaxParser();
            parser.Parse("");
        });

        return rootCommand.InvokeAsync(args).Result;
    }
}
