using Holoc;
using Holoc.Compile;
using System.CommandLine;
using System.Diagnostics;

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

        // 出力ディレクトリを取得
        var outDirPath = outDir ?? "output";

        // Holo compile stage

        var compiler = new HoloCompiler { Input = input, OutDirPath = outDirPath, ShowAst = showAst };
        compiler.Compile();

        if (compileFrontend) return;

        // C compile stage

        var quotedPaths = new List<string>();
        foreach (var path in compiler.CFileList)
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
