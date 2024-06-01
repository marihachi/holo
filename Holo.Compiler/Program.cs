using System;
using System.CommandLine;
using System.IO.MemoryMappedFiles;
using Holo.Compiler.Syntax;

public class Program
{
    static int Main(string[] args)
    {
        // Define a command
        var command = new RootCommand("The holo compiler");

        var inputArg = new Argument<string[]>("input", "list of input files.");
        command.Add(inputArg);

        var outputOption = new Option<string>("-o", "output filename.");
        command.Add(outputOption);

        // Set a handler will be called after command parsing
        command.SetHandler(ctx =>
        {
            var inputValues = ctx.ParseResult.GetValueForArgument(inputArg);
            var outputValues = ctx.ParseResult.GetValueForOption(outputOption);

            ProcessCommand(inputValues, outputValues);
        });

        // Execute the command parsing
        return command.InvokeAsync(args).Result;
    }

    static void ProcessCommand(string[] input, string? output)
    {
        var parser = new Parser();

        foreach (var filepath in input)
        {
            // Open a file as MMF stream
            using var mmf = MemoryMappedFile.CreateFromFile(filepath);
            using var stream = mmf.CreateViewStream();

            // Parse .holo file
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

            // TODO: Resolve

            // TODO: Emit LLVM-IR
        }
    }
}
