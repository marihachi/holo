using Holoc.Compile.Syntax.Node;
using Holoc.Compile.Syntax.Token;

namespace Holoc.Compile.Syntax;

public partial class Parser
{
    // WIP

    /// <summary>
    /// 式
    /// </summary>
    private SyntaxNode? ParseExpression()
    {
        return ParsePratt(0);
    }

    class PrefixOperatorInfo
    {
        public required SyntaxToken OperatorToken;
        public required int BindPower;
    }

    class InfixOperatorInfo
    {
        public required SyntaxToken OperatorToken;
        public required int LeftBindPower;
        public required int RightBindPower;
    }

    class PostfixOperatorInfo
    {
        public required SyntaxToken OperatorToken;
        public required int BindPower;
    }

    private List<PrefixOperatorInfo> PrefixOperators = [];
    private List<InfixOperatorInfo> InfixOperators = [];
    private List<PostfixOperatorInfo> PostfixOperators = [];

    private SyntaxNode? ParsePratt(int minimumBindPower)
    {
        // pratt parsing
        // https://matklad.github.io/2020/04/13/simple-but-powerful-pratt-parsing.html

        SyntaxNode? left = null;

        // find prefix operator
        PrefixOperatorInfo? prefix = null;

        if (prefix != null)
        {
            left = ParsePrefix();
            if (left == null) return null;
        }
        else
        {
            left = ParseAtom();
            if (left == null) return null;
        }

        while (true)
        {
            // find postfix operator
            PostfixOperatorInfo? postfix = null;

            if (postfix != null)
            {
                left = ParsePostfix();
                if (left == null) return null;
                continue;
            }

            // find infix operator
            InfixOperatorInfo? infix = null;

            if (infix != null)
            {
                left = ParseInfix();
                if (left == null) return null;
                continue;
            }

            break;
        }

        return left;
    }

    private SyntaxNode? ParsePrefix()
    {
        return null;
    }

    private SyntaxNode? ParsePostfix()
    {
        return null;
    }

    private SyntaxNode? ParseInfix()
    {
        return null;
    }

    private SyntaxNode? ParseAtom()
    {
        if (Try(TokenKind.NumberLiteral))
        {
            var location = CreateLocation();
            location.MarkBegin(Reader);

            var value = GetTokenValue<int>();
            if (!Next()) return null;

            location.MarkEnd(Reader);

            return SyntaxNode.CreateNumberLiteral(value, location);
        }

        GenerateError(Reader.CreateUnexpectedError());
        return null;
    }
}
