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
        if (!NextWith(TokenKind.SemiColon)) return null;

        location.MarkEnd(Reader);

        return SyntaxNode.CreateReturnStatement(null, location);
    }

    private SyntaxNode? ParseVariableDeclaration()
    {
        var location = CreateLocation();
        location.MarkBegin(Reader);

        if (!NextWith("var")) return null;

        if (!Expect(TokenKind.Word)) return null;
        var name = (string)Reader.Token!.Value!;
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

        var condition = ParseExpression();
        if (condition == null) return null;

        var body = ParseBlockOrStatement();
        if (body == null) return null;

        location.MarkEnd(Reader);

        return SyntaxNode.CreateWhileStatement(condition, body, location);
    }
}
