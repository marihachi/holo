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
        var name = GetTokenValue();
        if (!Next()) return null;

        location.MarkEnd(Reader);
        return SyntaxNode.CreateTypeReference(name, location);
    }

    /// <summary>
    /// ブロックまたは文をパースします。
    /// </summary>
    private SyntaxNode? ParseBlockOrStatement()
    {
        if (Try(TokenKind.OpenBrace))
        {
            var blockLocation = CreateLocation();
            blockLocation.MarkBegin(Reader);

            var nodeList = ParseBlock();
            if (nodeList == null) return null;

            blockLocation.MarkEnd(Reader);

            return SyntaxNode.CreateBlock(nodeList, blockLocation);
        }

        return ParseStatement();
    }

    /// <summary>
    /// ブロックをパースします。
    /// </summary>
    private List<SyntaxNode>? ParseBlock()
    {
        if (!NextWith(TokenKind.OpenBrace)) return null;

        var children = Repeat(ParseStatement, x => x.Kind == TokenKind.CloseBrace);
        if (children == null) return null;

        if (!NextWith(TokenKind.CloseBrace)) return null;

        return children;
    }
}
