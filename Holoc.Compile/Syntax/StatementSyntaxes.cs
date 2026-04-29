using Holoc.Compile.Syntax.Node;
using Holoc.Compile.Syntax.Token;

namespace Holoc.Compile.Syntax;

public partial class Parser
{
    /// <summary>
    /// 文
    /// </summary>
    private SyntaxNode? ParseStatement()
    {
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
            return ParseReturn();
        }

        if (Try("var"))
        {
            return ParseVariableDeclaration();
        }

        if (Try("while"))
        {
            return ParseWhileStatement();
        }

        if (Try(TokenKind.OpenBrace))
        {
            var blockLocation = CreateLocation();
            blockLocation.MarkBegin(Reader);

            var nodeList = ParseBlock();
            if (nodeList == null) return null;

            blockLocation.MarkEnd(Reader);

            return SyntaxNode.CreateBlock(nodeList, blockLocation);
        }

        // if statement

        // switch statement

        // expression statement

        GenerateError(Reader.CreateUnexpectedError());
        return null;
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
    private SyntaxNode? ParseReturn()
    {
        var location = CreateLocation();
        location.MarkBegin(Reader);

        if (!NextWith("return")) return null;

        SyntaxNode? expr = null;
        if (Try(TokenKind.SemiColon))
        {
            if (!Next()) return null;
        }
        else
        {
            expr = ParseExpression();
            if (expr == null) return null;
        }

        location.MarkEnd(Reader);

        return SyntaxNode.CreateReturnStatement(expr, location);
    }

    private SyntaxNode? ParseVariableDeclaration()
    {
        var location = CreateLocation();
        location.MarkBegin(Reader);

        if (!NextWith("var")) return null;

        if (!Expect(TokenKind.Word)) return null;
        var name = GetTokenValue();
        if (!Next()) return null;

        SyntaxNode? variableType = null;
        if (Try(TokenKind.Colon))
        {
            if (!Next()) return null;

            variableType = ParseTypeReference();
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

        return SyntaxNode.CreateVariableDecl(name, variableType, initializer, location);
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
}
