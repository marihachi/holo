using Holoc.Compile.C;
using Holoc.Compile.Holo;
using Holoc.Compile.Holo.Syntax.Node;
using Holoc.Compile.Syntax;
using System.Text;

namespace Holoc.Compile;

public class HoloCompiler
{
    public required string[] Input { get; set; }
    public required string OutDirPath { get; set; }
    public required bool ShowAst { get; set; }

    public List<string> CFileList { get; private set; } = new List<string>();

    public void Compile()
    {
        CFileList.Clear();

        var parser = new Parser();

        Directory.CreateDirectory(OutDirPath);

        foreach (var filePath in Input)
        {
            ISyntaxNode? unitNode;
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

            if (ShowAst && unitNode != null)
            {
                SyntaxView.Show(unitNode);
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
            var implFilePath = Path.Combine(OutDirPath, Path.ChangeExtension(holoFileName, ".c"));
            File.WriteAllText(implFilePath, implStr, Encoding.UTF8);
            CFileList.Add(implFilePath);

            // write header file
            var headerFilePath = Path.Combine(OutDirPath, Path.ChangeExtension(holoFileName, ".h"));
            File.WriteAllText(headerFilePath, cHeaderStr, Encoding.UTF8);
        }
    }
}
