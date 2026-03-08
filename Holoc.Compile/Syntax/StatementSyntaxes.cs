using Holoc.Compile.Syntax.Node;
using Holoc.Compile.Syntax.Token;

namespace Holoc.Compile.Syntax;

public partial class Parser
{
    /// <summary>
    /// 文
    /// </summary>
    private void ParseStatement()
    {
        Result = null;

        if (Try("break"))
        {
            ParseBreakStatement();
            return;
        }

        if (Try("continue"))
        {
            ParseContinueStatement();
            return;
        }

        if (Try("return"))
        {
            ParseReturn();
            return;
        }

        if (Try("var"))
        {
            ParseVariableDeclaration();
            return;
        }

        if (Try("while"))
        {
            ParseWhileStatement();
            return;
        }

        GenerateError(Reader.CreateUnexpectedError());
    }

    /// <summary>
    /// break文
    /// </summary>
    private void ParseBreakStatement()
    {
        Result = null;
        var location = CreateLocation();
        location.MarkBegin(Reader);

        if (!NextWith("break")) return;
        if (!NextWith(TokenKind.SemiColon)) return;

        location.MarkEnd(Reader);
        Result = SyntaxNode.CreateBreakStatement(location);
    }

    /// <summary>
    /// continue文
    /// </summary>
    private void ParseContinueStatement()
    {
        Result = null;
        var location = CreateLocation();
        location.MarkBegin(Reader);

        if (!NextWith("continue")) return;
        if (!NextWith(TokenKind.SemiColon)) return;

        location.MarkEnd(Reader);
        Result = SyntaxNode.CreateContinueStatement(location);
    }

    /// <summary>
    /// return文
    /// </summary>
    private void ParseReturn()
    {
        Result = null;
        var location = CreateLocation();
        location.MarkBegin(Reader);

        if (!NextWith("return")) return;
        if (!NextWith(TokenKind.SemiColon)) return;

        location.MarkEnd(Reader);
        Result = SyntaxNode.CreateReturnStatement(null, location);
    }

    private void ParseVariableDeclaration()
    {
        Result = null;
        var location = CreateLocation();
        location.MarkBegin(Reader);

        if (!NextWith("var")) return;

        if (!Expect(TokenKind.Word)) return;
        var name = (string)Reader.Token!.Value!;
        if (!Next()) return;

        SyntaxNode? variableType;
        if (Try(TokenKind.Colon))
        {
            if (!Next()) return;

            ParseTypeReference();
            if (Result == null) return;
            variableType = Result;
        }
        else
        {
            variableType = null;
        }

        SyntaxNode? initializer;
        if (Try(TokenKind.Eq))
        {
            if (!Next()) return;

            ParseExpression();
            if (Result == null) return;
            initializer = Result;
        }
        else
        {
            initializer = null;
        }

        location.MarkEnd(Reader);
        Result = SyntaxNode.CreateVariableDecl(name, variableType, initializer, location);
    }

    private void ParseWhileStatement()
    {
        Result = null;
        var location = CreateLocation();
        location.MarkBegin(Reader);

        if (!NextWith("while")) return;

        ParseExpression();
        if (Result == null) return;
        var condition = Result;

        SyntaxNode? body;
        var bodyLocation = CreateLocation();
        bodyLocation.MarkBegin(Reader);
        ParseBlockOrStatement();
        bodyLocation.MarkEnd(Reader);
        if (Results != null)
        {
            body = SyntaxNode.CreateBlock(Results, bodyLocation);
        }
        else if (Result != null)
        {
            body = Result;
        }
        else
        {
            return;
        }

        location.MarkEnd(Reader);
        Result = SyntaxNode.CreateWhileStatement(condition, body, location);
    }
}
