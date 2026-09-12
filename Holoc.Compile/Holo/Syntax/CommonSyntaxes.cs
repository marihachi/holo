using Holoc.Compile.Holo.Syntax.Node;
using Holoc.Compile.Holo.Syntax.Token;

namespace Holoc.Compile.Syntax;

public partial class Parser
{
    private ISyntaxNode? ParseType()
    {
        ISyntaxNode? outerNode = null;

        while (true)
        {
            if (Try(TokenKind.OpenBracket))
            {
                var location = CreateLocation();
                location.MarkBegin(Reader);
                if (!NextWith(TokenKind.OpenBracket)) return null;

                long? size = null;
                if (Try(TokenKind.NumberLiteral))
                {
                    size = GetTokenValue<long>();
                    if (!Next()) return null;
                }

                if (!NextWith(TokenKind.CloseBracket)) return null;
                location.MarkEnd(Reader);
                outerNode = new SyntaxCollectionType(outerNode, size, location);
                continue;
            }

            if (Try(TokenKind.Asterisk))
            {
                var location = CreateLocation();
                location.MarkBegin(Reader);
                if (!Next()) return null;
                location.MarkEnd(Reader);
                outerNode = new SyntaxPointerType(outerNode, location);
                continue;
            }

            if (Try(TokenKind.Word))
            {
                var location = CreateLocation();
                location.MarkBegin(Reader);
                var name = GetTokenValue();
                if (!Next()) return null;
                location.MarkEnd(Reader);
                outerNode = new SyntaxNamedType(name, location);
                continue;
            }

            break;
        }

        if (outerNode == null)
        {
            GenerateError(Reader.CreateUnexpectedError());
            return null;
        }

        return outerNode;
    }

    /// <summary>
    /// ブロックをパースします。
    /// </summary>
    private List<ISyntaxNode>? ParseBlock()
    {
        if (!NextWith(TokenKind.OpenBrace)) return null;

        var children = Repeat(ParseStatement, x => x.Kind == TokenKind.CloseBrace);
        if (children == null) return null;

        if (!NextWith(TokenKind.CloseBrace)) return null;

        return children;
    }
}
