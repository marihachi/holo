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
        return ParsePratt(0);
    }

    private class SingleOperatorInfo
    {
        public TokenKind OperatorToken;
        public int BindPower;

        public SingleOperatorInfo(TokenKind operatorToken, int bindPower)
        {
            OperatorToken = operatorToken;
            BindPower = bindPower;
        }
    }

    private class InfixOperatorInfo
    {
        public TokenKind OperatorToken;
        public int LeftBindPower;
        public int RightBindPower;

        public InfixOperatorInfo(TokenKind operatorToken, int leftBindPower, int rightBindPower)
        {
            OperatorToken = operatorToken;
            LeftBindPower = leftBindPower;
            RightBindPower = rightBindPower;
        }
    }

    private List<SingleOperatorInfo> PrefixOperators = [
        //new(TokenKind.Hat, 80),
        //new(TokenKind.Not, 80),
        //new(TokenKind.Plus, 80),
        //new(TokenKind.Minus, 80),
    ];

    private List<InfixOperatorInfo> InfixOperators = [
        //new(TokenKind.Dot, 90, 91),
        new(TokenKind.Asterisk, 70, 71),
        new(TokenKind.Slash, 70, 71),
        new(TokenKind.Plus, 60, 61),
        new(TokenKind.Minus, 60, 61),
    ];

    private List<SingleOperatorInfo> PostfixOperators = [
        //new(TokenKind.OpenParen, 90),
        //new(TokenKind.OpenBracket, 90),
    ];

    private SyntaxNode? ParsePratt(int minimumBindPower)
    {
        // pratt parsing
        // https://matklad.github.io/2020/04/13/simple-but-powerful-pratt-parsing.html

        SyntaxNode? left = null;

        var kind = GetKind();

        // find prefix operator
        SingleOperatorInfo? prefix = PrefixOperators.Find(x => x.OperatorToken == kind);

        if (prefix != null)
        {
            left = ParsePrefix(prefix);
            if (left == null) return null;
        }
        else
        {
            left = ParseAtom();
            if (left == null) return null;
        }

        while (true)
        {
            kind = GetKind();

            // find postfix operator
            SingleOperatorInfo? postfix = PostfixOperators.Find(x => x.OperatorToken == kind);

            if (postfix != null)
            {
                if (postfix.BindPower < minimumBindPower)
                {
                    break;
                }

                left = ParsePostfix(postfix, left);
                if (left == null) return null;

                continue;
            }

            // find infix operator
            InfixOperatorInfo? infix = InfixOperators.Find(x => x.OperatorToken == kind);

            if (infix != null)
            {
                if (infix.LeftBindPower < minimumBindPower)
                {
                    break;
                }

                left = ParseInfix(infix, left);
                if (left == null) return null;

                continue;
            }

            break;
        }

        return left;
    }

    private SyntaxNode? ParsePrefix(SingleOperatorInfo operatorInfo)
    {
        // TODO
        return null;
    }

    private SyntaxNode? ParsePostfix(SingleOperatorInfo operatorInfo, SyntaxNode left)
    {
        // TODO
        return null;
    }

    private SyntaxNode? ParseInfix(InfixOperatorInfo operatorInfo, SyntaxNode left)
    {
        var location = CreateLocation();
        location.MarkBegin(Reader);

        if (!Next()) return null;

        location.MarkEnd(Reader);

        var right = ParsePratt(operatorInfo.RightBindPower);
        if (right == null) return null;

        NodeMode mode;
        if (operatorInfo.OperatorToken == TokenKind.Plus)
        {
            mode = NodeMode.Add;
        }
        else if (operatorInfo.OperatorToken == TokenKind.Minus)
        {
            mode = NodeMode.Sub;
        }
        else if (operatorInfo.OperatorToken == TokenKind.Asterisk)
        {
            mode = NodeMode.Mul;
        }
        else if (operatorInfo.OperatorToken == TokenKind.Slash)
        {
            mode = NodeMode.Div;
        }
        else
        {
            return null;
        }
        
        return SyntaxNode.CreateBinaryOperation(mode, left, right, location);
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

        if (Try("if"))
        {
            return ParseIfExpression();
        }

        if (Try("switch"))
        {
            return ParseSwitchExpression();
        }

        if (Try(TokenKind.Word))
        {
            var location = CreateLocation();
            location.MarkBegin(Reader);

            var name = GetTokenValue<string>();
            if (!Next()) return null;

            location.MarkEnd(Reader);

            return SyntaxNode.CreateReference(name, location);
        }

        // block expression
        if (Try(TokenKind.OpenBrace))
        {
            var location = CreateLocation();
            location.MarkBegin(Reader);

            var nodeList = ParseBlock();
            if (nodeList == null) return null;

            location.MarkEnd(Reader);

            return SyntaxNode.CreateBlockExpression(nodeList, location);
        }

        GenerateError(Reader.CreateUnexpectedError());
        return null;
    }

    /// <summary>
    /// if式
    /// </summary>
    private SyntaxNode? ParseIfExpression()
    {
        var location = CreateLocation();
        location.MarkBegin(Reader);

        if (!NextWith("if")) return null;

        if (!NextWith(TokenKind.OpenParen)) return null;
        var condExpr = ParseExpression();
        if (condExpr == null) return null;
        if (!NextWith(TokenKind.CloseParen)) return null;

        var thenExpr = ParseExpression();
        if (thenExpr == null) return null;

        SyntaxNode? elseExpr = null;
        if (Try("else"))
        {
            if (!Next()) return null;

            elseExpr = ParseExpression();
            if (elseExpr == null) return null;
        }

        location.MarkEnd(Reader);

        return SyntaxNode.CreateIfExpression(condExpr, thenExpr, elseExpr, location);
    }

    /// <summary>
    /// switch式
    /// </summary>
    private SyntaxNode? ParseSwitchExpression()
    {
        var location = CreateLocation();
        location.MarkBegin(Reader);

        if (!NextWith("switch")) return null;

        if (!NextWith(TokenKind.OpenParen)) return null;
        var condExpr = ParseExpression();
        if (condExpr == null) return null;
        if (!NextWith(TokenKind.CloseParen)) return null;

        if (!NextWith(TokenKind.OpenBrace)) return null;

        // TODO: arms
        var arms = new List<SyntaxNode>();

        if (!NextWith(TokenKind.CloseBrace)) return null;

        location.MarkEnd(Reader);

        return SyntaxNode.CreateSwitchExpression(condExpr, arms, location);
    }
}
