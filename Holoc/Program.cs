using Holoc;
using Holoc.Compile.CLang;
using Holoc.Compile.IR;
using Holoc.Compile.Syntax;
using Holoc.Compile.Syntax.Node;
using System.CommandLine;
using System.Diagnostics;
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
        command.SetHandler(async ctx =>
        {
            var inputValues = ctx.ParseResult.GetValueForArgument(inputArg);
            var outputValue = ctx.ParseResult.GetValueForOption(outputOption);
            var showAstValue = ctx.ParseResult.GetValueForOption(showAstOption);

            await ProcessCommand(inputValues, outputValue, showAstValue);
        });

        // Execute the command parsing
        return command.InvokeAsync(args).Result;
    }

    static async Task ProcessCommand(string[] input, string? output, string? showAst)
    {
        if (input.Length == 0)
        {
            Console.Error.WriteLine("No input files specified.");
            return;
        }

        var config = HoloConfigLoader.Load();
        var parser = new Parser();

        // objフォルダを取得
        var objDirPath = config.ObjDir;
        Directory.CreateDirectory(objDirPath);

        var cFileList = new List<string>();
        foreach (var filePath in input)
        {
            // Open a file as MMF stream
            // .holo files may be large and should be read in small (1KB) blocks to avoid memory pressure.
            //using var mmf = MemoryMappedFile.CreateFromFile(filepath);
            //using var stream = mmf.CreateViewStream();

            SyntaxNode? unitNode;
            using (var reader = new StreamReader(
                filePath,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true,
                bufferSize: 1024 * 1024) // 1MB
            )
            {
                // Parse .holo file
                unitNode = parser.Parse(reader);
            }

            if (parser.Errors.Count > 0)
            {
                Console.Error.WriteLine("Syntax error.");
                foreach (var error in parser.Errors)
                {
                    Console.Error.WriteLine(error);
                }
            }

            if (showAst != null && unitNode != null)
            {
                SyntaxNode.ShowSyntaxNode(unitNode);
            }

            if (unitNode == null) return;

            // AST -> Holo IR
            var holoIrBuilder = new HoloIRBuilder();
            holoIrBuilder.Build(unitNode);
            var holoIr = holoIrBuilder.HoloUnit;

            // TODO: semantic analysis

            // Holo IR -> C IR
            var cIrBuilder = new CIRBuilder();
            cIrBuilder.Build(holoIr);
            var cUnit = cIrBuilder.CUnit;

            // C IR -> C code
            var cCode = new CEmitter().Emit(cUnit);

            // write C code
            var cFilePath = Path.Combine(objDirPath, Path.GetFileName(Path.ChangeExtension(filePath, ".c")));
            File.WriteAllText(cFilePath, cCode, Encoding.UTF8);

            cFileList.Add(cFilePath);
        }

        var quotedPaths = new List<string>();
        foreach (var path in cFileList)
        {
            quotedPaths.Add($"\"{path}\"");
        }
        var sourceFiles = string.Join(" ", quotedPaths);
        var binPath = Path.ChangeExtension(output, ".exe");

        // compile and link
        var clangArgs = $"{sourceFiles} -o {binPath}";
        if (!string.IsNullOrWhiteSpace(config.ClangExtraArgs))
        {
            clangArgs += $" {config.ClangExtraArgs}";
        }

        await Process.Start(new ProcessStartInfo
        {
            FileName = config.ClangPath,
            Arguments = clangArgs,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        })!.WaitForExitAsync();
    }
}
