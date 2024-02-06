using System;
using System.CommandLine;

public class Program
{
    static int Main(string[] args)
    {
        var rootCommand = new RootCommand("The holo compiler");

        var inputArg = new Argument<string[]>("input", "list of input files.");
        rootCommand.Add(inputArg);

        rootCommand.SetHandler((ctx) =>
        {
            var inputValue = ctx.ParseResult.GetValueForArgument(inputArg);
            // do something
        });

        return rootCommand.InvokeAsync(args).Result;
    }
}
