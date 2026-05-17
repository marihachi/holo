using Holoc;
using Holoc.Compile.C;
using Holoc.Compile.Holo;
using Holoc.Compile.Holo.Syntax.Node;
using Holoc.Compile.Syntax;
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

        var outputOption = new Option<string>(["-o", "--outFile"], "specify a filename of an executable file.");
        command.Add(outputOption);

        var outDirOption = new Option<string>("--outDir", "specify output directory.");
        command.Add(outDirOption);

        var CompileFrontendOption = new Option<bool>(["-f", "--frontend"], "execute the frontend stage only.");
        command.Add(CompileFrontendOption);

        var showAstOption = new Option<bool>("--ast", "show syntax tree of the input files.");
        command.Add(showAstOption);

        // Set a handler will be called after command parsing
        command.SetHandler(async ctx =>
        {
            var inputValues = ctx.ParseResult.GetValueForArgument(inputArg);
            var outputValue = ctx.ParseResult.GetValueForOption(outputOption);
            var CompileFrontendValue = ctx.ParseResult.GetValueForOption(CompileFrontendOption);
            var outDirValue = ctx.ParseResult.GetValueForOption(outDirOption);
            var showAstValue = ctx.ParseResult.GetValueForOption(showAstOption);

            await ProcessCommand(
                inputValues,
                outputValue,
                CompileFrontendValue,
                outDirValue,
                showAstValue
                );
        });

        // Execute the command parsing
        return command.InvokeAsync(args).Result;
    }

    static async Task ProcessCommand(
        string[] input,
        string? output,
        bool compileFrontend,
        string? outDir,
        bool showAst
        )
    {
        if (input.Length == 0)
        {
            Console.Error.WriteLine("No input files specified.");
            return;
        }

        var config = HoloConfigLoader.Load();
        var parser = new Parser();

        // 出力ディレクトリを取得
        var outDirPath = outDir ?? "output";
        Directory.CreateDirectory(outDirPath);

        // frontend stage

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

            if (showAst && unitNode != null)
            {
                SyntaxNode.ShowSyntaxNode(unitNode);
            }

            if (unitNode == null) return;

            var holoFileName = Path.GetFileName(filePath);

            // AST -> Holo IR
            var holoIrBuilder = new HoloIRBuilder();
            holoIrBuilder.Build(holoFileName, unitNode);
            var holoIr = holoIrBuilder.HoloUnit;

            // TODO: semantic analysis

            // Holo IR -> C IR
            var cIrBuilder = new CSyntaxNodeBuilder();
            cIrBuilder.Build(holoIr);
            var cImpl = cIrBuilder.CImpl;
            var cHeader = cIrBuilder.CHeader;

            // C AST -> C file
            var implStr = new CEmitter().Emit(cImpl);
            var cHeaderStr = new CEmitter().Emit(cHeader);

            // write implement file
            var implFilePath = Path.Combine(outDirPath, Path.ChangeExtension(holoFileName, ".c"));
            File.WriteAllText(implFilePath, implStr, Encoding.UTF8);
            cFileList.Add(implFilePath);

            // write header file
            var headerFilePath = Path.Combine(outDirPath, Path.ChangeExtension(holoFileName, ".h"));
            File.WriteAllText(headerFilePath, cHeaderStr, Encoding.UTF8);
        }

        if (compileFrontend) return;

        // C compile stage

        var quotedPaths = new List<string>();
        foreach (var path in cFileList)
        {
            quotedPaths.Add($"\"{path}\"");
        }
        var sourceFiles = string.Join(" ", quotedPaths);

        // build arguments of C compiler
        var clangArgs = $"{sourceFiles}";
        clangArgs += $" -o {Path.Combine(outDirPath, output ?? "a.exe")}";
        if (!string.IsNullOrWhiteSpace(config.ClangExtraArgs))
        {
            clangArgs += $" {config.ClangExtraArgs}";
        }

        // compile C files, and link object files
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
