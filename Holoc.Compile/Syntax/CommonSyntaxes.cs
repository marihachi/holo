using Holoc.Compile.Syntax.Node;
using Holoc.Compile.Syntax.Token;

namespace Holoc.Compile.Syntax;

public partial class Parser
{
    private SyntaxNode? ParseTypeReference()
    {
        var location = CreateLocation();
        location.MarkBegin(Reader);

        if (!Expect(TokenKind.Word)) return null;
        var name = (string)Reader.Token!.Value!;
        if (!Next()) return null;

        location.MarkEnd(Reader);
        return SyntaxNode.CreateTypeReference(name, location);
    }

    /// <summary>
    /// ブロックまたは文をパースします。
    /// ブロックの場合、結果はList<SyntaxNode>が返されます。
    /// 文の場合、結果はSyntaxNodeが返されます。
    /// </summary>
    private object? ParseBlockOrStatement()
    {
        if (Try(TokenKind.OpenBrace))
        {
            return ParseBlock();
        }

        return ParseStatement();
    }

    /// <summary>
    /// ブロックをパースします。
    /// </summary>
    private List<SyntaxNode>? ParseBlock()
    {
        List<SyntaxNode>? results;

        if (!NextWith(TokenKind.OpenBrace)) return null;

        results = Repeat(ParseStatement, x => x.Kind == TokenKind.CloseBrace);
        if (results == null) return null;

        if (!NextWith(TokenKind.CloseBrace)) return null;

        return results;
    }
}
