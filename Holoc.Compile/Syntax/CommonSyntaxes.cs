using Holoc.Compile.Syntax.Node;
using Holoc.Compile.Syntax.Token;

namespace Holoc.Compile.Syntax;

public partial class Parser
{
    private void ParseTypeReference()
    {
        Result = null;
        var location = CreateLocation();
        location.MarkBegin(Reader);

        if (!Expect(TokenKind.Word)) return;
        var name = (string)Reader.Token!.Value!;
        if (!Next()) return;

        location.MarkEnd(Reader);
        Result = SyntaxNode.CreateTypeReference(name, location);
    }

    /// <summary>
    /// ブロックまたは文をパースします。
    /// ブロックの場合、結果はResultsにセットされます。
    /// 文の場合、結果はResultにセットされます。
    /// </summary>
    private void ParseBlockOrStatement()
    {
        Result = null;
        Results = null;

        if (Try(TokenKind.OpenBrace))
        {
            ParseBlock();
            return;
        }

        ParseStatement();
    }

    /// <summary>
    /// ブロックをパースします。
    /// 結果はResultsにセットされます。
    /// </summary>
    private void ParseBlock()
    {
        Results = null;

        if (!NextWith(TokenKind.OpenBrace)) return;

        Repeat(ParseStatement, x => x.Kind == TokenKind.CloseBrace);
        if (Results == null) return;

        if (!NextWith(TokenKind.CloseBrace)) return;
    }
}
