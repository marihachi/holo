using Holoc.Compile.Holo.Syntax.Node;
using Holoc.Compile.Holo.Syntax.Token;

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
        new(TokenKind.Plus, 80),
        new(TokenKind.Minus, 80),
    ];

    private List<InfixOperatorInfo> InfixOperators = [
        //new(TokenKind.Dot, 90, 91),
        new(TokenKind.Asterisk, 70, 71),
        new(TokenKind.Slash, 70, 71),
        new(TokenKind.Plus, 60, 61),
        new(TokenKind.Minus, 60, 61),
        new(TokenKind.Gt, 50, 51),
        new(TokenKind.GtEq, 50, 51),
        new(TokenKind.Lt, 50, 51),
        new(TokenKind.LtEq, 50, 51),
        new(TokenKind.Eq2, 40, 41),
        new(TokenKind.NotEq, 40, 41),
    ];

    private List<SingleOperatorInfo> PostfixOperators = [
        new(TokenKind.OpenParen, 90),
        new(TokenKind.OpenBracket, 90),
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
        var location = CreateLocation();
        location.MarkBegin(Reader);

        if (!Next()) return null;

        var right = ParsePratt(operatorInfo.BindPower);
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
        else
        {
            return null;
        }

        location.MarkEnd(Reader);

        return SyntaxNode.CreateUnaryOperation(mode, right, location);
    }

    private SyntaxNode? ParsePostfix(SingleOperatorInfo operatorInfo, SyntaxNode left)
    {
        var location = CreateLocation();
        location.MarkBegin(Reader);

        if (!Next()) return null;

        // call
        if (operatorInfo.OperatorToken == TokenKind.OpenParen)
        {
            var args = Repeat(ParseExpression, t => t.Kind == TokenKind.CloseParen, s => s.Kind == TokenKind.Comma);
            if (args == null) return null;

            if (!NextWith(TokenKind.CloseParen)) return null;

            location.MarkEnd(Reader);

            return SyntaxNode.CreateCall(left, args, location);
        }

        // index reference
        if (operatorInfo.OperatorToken == TokenKind.OpenBracket)
        {
            var indexExpr = ParseExpression();
            if (indexExpr == null) return null;

            if (!NextWith(TokenKind.CloseBracket)) return null;

            location.MarkEnd(Reader);
            return SyntaxNode.CreateIndexRef(left, indexExpr, location);
        }

        return null;
    }

    private static readonly Dictionary<TokenKind, NodeMode> InfixToModeTable = new()
    {
        { TokenKind.Plus, NodeMode.Add },
        { TokenKind.Minus, NodeMode.Sub },
        { TokenKind.Asterisk, NodeMode.Mul },
        { TokenKind.Slash, NodeMode.Div },
        { TokenKind.Gt, NodeMode.Gt },
        { TokenKind.Lt, NodeMode.Lt },
        { TokenKind.GtEq, NodeMode.GtEq },
        { TokenKind.LtEq, NodeMode.LtEq },
        { TokenKind.Eq2, NodeMode.Eq },
        { TokenKind.NotEq, NodeMode.NotEq },
    };

    private SyntaxNode? ParseInfix(InfixOperatorInfo operatorInfo, SyntaxNode left)
    {
        var location = CreateLocation();
        location.MarkBegin(Reader);

        if (!Next()) return null;

        location.MarkEnd(Reader);

        var right = ParsePratt(operatorInfo.RightBindPower);
        if (right == null) return null;

        // 演算子トークンからモードへ変換
        if (!InfixToModeTable.TryGetValue(operatorInfo.OperatorToken, out NodeMode mode))
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

            var value = GetTokenValue<long>();
            if (!Next()) return null;

            location.MarkEnd(Reader);

            return SyntaxNode.CreateNumberLiteral(value, location);
        }

        if (Try("when"))
        {
            return ParseWhenExpression();
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

        // collection
        if (Try(TokenKind.OpenBracket))
        {
            if (!Next()) return null;

            var location = CreateLocation();
            location.MarkBegin(Reader);

            var elements = Repeat(ParseExpression, t => t.Kind == TokenKind.CloseBracket, s => s.Kind == TokenKind.Comma);
            if (elements == null) return null;

            if (!NextWith(TokenKind.CloseBracket)) return null;

            location.MarkEnd(Reader);

            return SyntaxNode.CreateCollectionExpression(elements, location);
        }

        GenerateError(Reader.CreateUnexpectedError());
        return null;
    }

    /// <summary>
    /// when式
    /// </summary>
    private SyntaxNode? ParseWhenExpression()
    {
        var location = CreateLocation();
        location.MarkBegin(Reader);

        var arms = new List<SyntaxNode>();
        while (Try("when"))
        {
            if (!Next()) return null;

            if (!NextWith(TokenKind.OpenParen)) return null;
            var condExpr = ParseExpression();
            if (condExpr == null) return null;
            if (!NextWith(TokenKind.CloseParen)) return null;

            var thenExpr = ParseExpression();
            if (thenExpr == null) return null;

            arms.Add(SyntaxNode.CreateWhenArm(false, condExpr, thenExpr, location));
        }

        if (Try("else"))
        {
            if (!Next()) return null;

            var elseExpr = ParseExpression();
            if (elseExpr == null) return null;

            arms.Add(SyntaxNode.CreateWhenArm(true, null, elseExpr, location));
        }

        location.MarkEnd(Reader);

        return SyntaxNode.CreateWhenExpression(arms, location);
    }
}
