using Holoc.Compile.Holo.Syntax.Node;
using Holoc.Compile.Holo.Syntax.Token;

namespace Holoc.Compile.Syntax;

public partial class Parser
{
    private SyntaxNode? ParseType()
    {
        SyntaxNode? outerNode = null;

        while (true)
        {
            if (Try(TokenKind.OpenBracket))
            {
                var location = CreateLocation();
                location.MarkBegin(Reader);
                if (!NextWith(TokenKind.OpenBracket)) return null;
                if (!NextWith(TokenKind.CloseBracket)) return null;
                location.MarkEnd(Reader);
                outerNode = SyntaxNode.CreateCollectionType(outerNode, location);
                continue;
            }

            if (Try(TokenKind.Word))
            {
                var location = CreateLocation();
                location.MarkBegin(Reader);
                if (!Expect(TokenKind.Word)) return null;
                var name = GetTokenValue();
                if (!Next()) return null;
                location.MarkEnd(Reader);
                outerNode = SyntaxNode.CreateNamedType(name, location);
                continue;
            }
            
            if (outerNode == null)
            {
                GenerateError(Reader.CreateUnexpectedError());
                return null;
            }

            return outerNode;
        }
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
