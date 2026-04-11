using Holoc.Compile.Syntax.Node;
using Holoc.Compile.Syntax.Token;

namespace Holoc.Compile.Syntax;

public partial class Parser
{
    /// <summary>
    /// 式
    /// </summary>
    private SyntaxNode? ParseExpression()
    {
        if (Try(TokenKind.NumberLiteral))
        {
            var location = CreateLocation();
            location.MarkBegin(Reader);
            var value = GetTokenValue();
            Next();
            location.MarkEnd(Reader);

            return SyntaxNode.CreateNumberLiteral(int.Parse(value), location);
        }

        GenerateError("Not implemented");
        return null;
    }
}
