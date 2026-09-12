using Holoc.Compile.Holo.Syntax.Node;
using Holoc.Compile.Holo.Syntax.Token;

namespace Holoc.Compile.Syntax;

public partial class Parser
{
    private ISyntaxNode? ParseType()
    {
        // 型名に続けて、後置の修飾子(配列、ポインタ)を任意の数だけ指定できます。

        // 型名
        var nameLocation = CreateLocation();
        nameLocation.MarkBegin(Reader);

        if (!Expect(TokenKind.Word)) return null;
        var name = GetTokenValue();
        if (!Next()) return null;

        nameLocation.MarkEnd(Reader);

        ISyntaxNode outerNode = new SyntaxNamedType(name, nameLocation);

        // 後置の修飾子
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

            break;
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
