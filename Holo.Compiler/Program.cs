using System;
using System.CommandLine;
using System.IO.MemoryMappedFiles;
using Holo.Compiler.Syntax;

public class Program
{
    static int Main(string[] args)
    {
        var rootCommand = new RootCommand("The holo compiler");

        var inputArg = new Argument<string[]>("input", "list of input files.");
        rootCommand.Add(inputArg);

        var outputOption = new Option<string>("-o", "output filename.");
        rootCommand.Add(outputOption);

        rootCommand.SetHandler(ctx =>
        {
            var input = ctx.ParseResult.GetValueForArgument(inputArg);
            var output = ctx.ParseResult.GetValueForOption(outputOption);

            ProcessCommand(input, output);
        });

        return rootCommand.InvokeAsync(args).Result;
    }

    static void ProcessCommand(string[] input, string? output)
    {
        var parser = new Parser();

        foreach (var filepath in input)
        {
            // open a file as MMF stream
            using var mmf = MemoryMappedFile.CreateFromFile(filepath);
            using var stream = mmf.CreateViewStream();

            // parse file
            parser.Parse(stream);

            if (!parser.IsSuccess)
            {
                Console.Error.WriteLine("parse failed:");
                foreach (var error in parser.Errors)
                {
                    Console.Error.WriteLine(error);
                }
                return;
            }

            var unitNode = parser.Result;

            // TODO: resolve

            // TODO: emit LLVM
        }
    }
}
