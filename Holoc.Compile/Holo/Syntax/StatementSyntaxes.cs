using Holoc.Compile.Holo.Syntax.Node;
using Holoc.Compile.Holo.Syntax.Token;

namespace Holoc.Compile.Syntax;

public partial class Parser
{
    /// <summary>
    /// 文
    /// </summary>
    private SyntaxNode? ParseStatement()
    {
        var isDeclare = false;

        // 修飾子は順不同
        while (true)
        {
            if (Try("declare"))
            {
                Next();
                isDeclare = true;
                continue;
            }

            break;
        }

        // declareは宣言にのみ指定可能
        if (isDeclare && !Try("var"))
        {
            GenerateError(Reader.CreateUnexpectedError());
            return null;
        }

        if (Try("break"))
        {
            return ParseBreakStatement();
        }

        if (Try("continue"))
        {
            return ParseContinueStatement();
        }

        if (Try("return"))
        {
            return ParseReturnStatement();
        }

        if (Try("var"))
        {
            var isExport = false;

            return ParseVariableDeclaration(isDeclare, isExport);
        }

        if (Try("while"))
        {
            return ParseWhileStatement();
        }

        if (Try("if"))
        {
            return ParseIfStatement();
        }

        var location = CreateLocation();
        location.MarkBegin(Reader);

        // 式文
        var expr = ParseExpression();
        if (expr != null)
        {
            if (expr.Kind == NodeKind.BlockExpression)
            {
                return expr;
            }

            if (Try(TokenKind.SemiColon))
            {
                if (!Next()) return null;

                location.MarkEnd(Reader);

                return SyntaxNode.CreateExpressionStatement(
                    expr,
                    location);
            }

            if (Try(TokenKind.Eq, TokenKind.PlusEq, TokenKind.MinusEq, TokenKind.AsterEq, TokenKind.SlashEq))
            {
                var nodeMode = ConvertToNodeMode(GetKind());

                if (!Next()) return null;

                var rightExpr = ParseExpression();
                if (rightExpr == null) return null;

                if (!NextWith(TokenKind.SemiColon)) return null;
                
                location.MarkEnd(Reader);
                
                return SyntaxNode.CreateAssignmentStatement(
                    nodeMode,
                    expr,
                    rightExpr,
                    location);
            }
        }

        GenerateError(Reader.CreateUnexpectedError());
        return null;
    }

    private Dictionary<TokenKind, NodeMode> NodeModeMap = new()
    {
        { TokenKind.Eq, NodeMode.None },
        { TokenKind.PlusEq, NodeMode.Add },
        { TokenKind.MinusEq, NodeMode.Sub },
        { TokenKind.AsterEq, NodeMode.Mul },
        { TokenKind.SlashEq, NodeMode.Div }
    };

    private NodeMode ConvertToNodeMode(TokenKind tokenKind)
    {
        if (!NodeModeMap.TryGetValue(tokenKind, out NodeMode mode))
        {
            throw new InvalidOperationException($"Unexpected token kind: {tokenKind}");
        }
        return mode;
    }

    /// <summary>
    /// break文
    /// </summary>
    private SyntaxNode? ParseBreakStatement()
    {
        var location = CreateLocation();
        location.MarkBegin(Reader);

        if (!NextWith("break")) return null;
        if (!NextWith(TokenKind.SemiColon)) return null;

        location.MarkEnd(Reader);

        return SyntaxNode.CreateBreakStatement(location);
    }

    /// <summary>
    /// continue文
    /// </summary>
    private SyntaxNode? ParseContinueStatement()
    {
        var location = CreateLocation();
        location.MarkBegin(Reader);

        if (!NextWith("continue")) return null;
        if (!NextWith(TokenKind.SemiColon)) return null;

        location.MarkEnd(Reader);

        return SyntaxNode.CreateContinueStatement(location);
    }

    /// <summary>
    /// return文
    /// </summary>
    private SyntaxNode? ParseReturnStatement()
    {
        var location = CreateLocation();
        location.MarkBegin(Reader);

        if (!NextWith("return")) return null;

        var isForceReturnFunc = false;
        if (Try("fn"))
        {
            if (!Next()) return null;

            isForceReturnFunc = true;
        }

        SyntaxNode? expr = null;
        if (Try(TokenKind.SemiColon))
        {
            if (!Next()) return null;
        }
        else
        {
            expr = ParseExpression();
            if (expr == null) return null;

            if (!NextWith(TokenKind.SemiColon)) return null;
        }

        location.MarkEnd(Reader);

        return SyntaxNode.CreateReturnStatement(expr, isForceReturnFunc, location);
    }

    private SyntaxNode? ParseVariableDeclaration(bool isDeclare, bool isExport)
    {
        var location = CreateLocation();
        location.MarkBegin(Reader);

        if (!Next()) return null;

        if (!Expect(TokenKind.Word)) return null;
        var name = GetTokenValue();
        if (!Next()) return null;

        SyntaxNode? variableType = null;
        if (Try(TokenKind.Colon))
        {
            if (!Next()) return null;

            variableType = ParseType();
            if (variableType == null) return null;
        }

        SyntaxNode? initializer = null;
        if (Try(TokenKind.Eq))
        {
            if (!Next()) return null;

            initializer = ParseExpression();
            if (initializer == null) return null;
        }

        if (!NextWith(TokenKind.SemiColon)) return null;

        location.MarkEnd(Reader);

        return SyntaxNode.CreateVariableDecl(name, variableType, initializer, isDeclare, isExport, location);
    }

    private SyntaxNode? ParseWhileStatement()
    {
        var location = CreateLocation();
        location.MarkBegin(Reader);

        if (!NextWith("while")) return null;

        if (!NextWith(TokenKind.OpenParen)) return null;

        var condition = ParseExpression();
        if (condition == null) return null;

        if (!NextWith(TokenKind.CloseParen)) return null;

        var body = ParseStatement();
        if (body == null) return null;

        location.MarkEnd(Reader);

        return SyntaxNode.CreateWhileStatement(condition, body, location);
    }

    /// <summary>
    /// if文
    /// </summary>
    private SyntaxNode? ParseIfStatement()
    {
        var location = CreateLocation();
        location.MarkBegin(Reader);

        if (!NextWith("if")) return null;

        if (!NextWith(TokenKind.OpenParen)) return null;
        var condExpr = ParseExpression();
        if (condExpr == null) return null;
        if (!NextWith(TokenKind.CloseParen)) return null;

        var thenStmt = ParseStatement();
        if (thenStmt == null) return null;

        SyntaxNode? elseStmt = null;
        if (Try("else"))
        {
            if (!Next()) return null;

            elseStmt = ParseStatement();
            if (elseStmt == null) return null;
        }

        location.MarkEnd(Reader);

        return SyntaxNode.CreateIfStatement(condExpr, thenStmt, elseStmt, location);
    }
}
