using Holoc.Compile.Syntax;
using Holoc.Compile.Syntax.Node;
using System.CommandLine;
using System.Text;

public class Program
{
    static int Main(string[] args)
    {
        // Define a command
        var command = new RootCommand("The Holo compiler");

        var inputArg = new Argument<string[]>("input", "list of input files.");
        command.Add(inputArg);

        var outputOption = new Option<string>("-o", "output filename.");
        command.Add(outputOption);

        var showAstOption = new Option<string>("--ast", "show AST.");
        command.Add(showAstOption);

        // Set a handler will be called after command parsing
        command.SetHandler(ctx =>
        {
            var inputValues = ctx.ParseResult.GetValueForArgument(inputArg);
            var outputValue = ctx.ParseResult.GetValueForOption(outputOption);
            var showAstValue = ctx.ParseResult.GetValueForOption(showAstOption);

            ProcessCommand(inputValues, outputValue, showAstValue);
        });

        // Execute the command parsing
        return command.InvokeAsync(args).Result;
    }

    static void ProcessCommand(string[] input, string? output, string? showAst)
    {
        if (input.Length == 0)
        {
            Console.Error.WriteLine("No input files specified.");
            return;
        }

        var parser = new Parser();

        foreach (var filepath in input)
        {
            // Open a file as MMF stream
            // .holo files may be large and should be read in small (1KB) blocks to avoid memory pressure.
            //using var mmf = MemoryMappedFile.CreateFromFile(filepath);
            //using var stream = mmf.CreateViewStream();

            SyntaxNode? unitNode;
            using (var reader = new StreamReader(
                filepath,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true,
                bufferSize: 1024 * 1024) // 1MB
                )
            {
                // Parse .holo file
                unitNode = parser.Parse(reader);
            }

            if (unitNode == null)
            {
                Console.Error.WriteLine("Syntax error.");
                foreach (var error in parser.Errors)
                {
                    Console.Error.WriteLine(error);
                }
                return;
            }

            if (showAst != null && unitNode != null)
            {
                SyntaxNode.ShowSyntaxNode(unitNode);
            }

            // TODO: Resolve

            // TODO: Emit LLVM-IR
        }
    }
}
