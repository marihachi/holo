using Holoc.Compile.Syntax.Node;
using Holoc.Compile.Syntax.Token;

namespace Holoc.Compile.Syntax;

public partial class Parser
{
    /// <summary>
    /// コンパイル単位
    /// </summary>
    private SyntaxNode? ParseUnit()
    {
        var location = CreateLocation();
        location.MarkBegin(Reader);

        var results = Repeat(ParseTopLevelDecl, x => x.Kind == TokenKind.EOF);
        if (results == null) return null;
        List<SyntaxNode> body = [];
        body.AddRange(results);

        location.MarkEnd(Reader);
        return SyntaxNode.CreateUnit(body, location);
    }

    /// <summary>
    /// トップレベル宣言
    /// </summary>
    private SyntaxNode? ParseTopLevelDecl()
    {
        var isExternal = false;
        if (Try("external"))
        {
            Next();
            isExternal = true;
        }

        if (Try("fn"))
        {
            return ParseFunctionDecl(isExternal);
        }

        if (Try("var") && !isExternal)
        {
            return ParseVariableDeclaration();
        }

        GenerateError(Reader.CreateUnexpectedError());
        return null;
    }

    /// <summary>
    /// 関数宣言
    /// </summary>
    private SyntaxNode? ParseFunctionDecl(bool isExternal)
    {
        List<SyntaxNode>? results;

        var location = CreateLocation();
        location.MarkBegin(Reader);

        if (!NextWith("fn")) return null;

        // name
        if (!Expect(TokenKind.Word)) return null;
        var name = GetTokenValue();
        if (!Next()) return null;

        // parameters
        if (!NextWith(TokenKind.OpenParen)) return null;
        results = Repeat(ParseFunctionParameter, x => x.Kind == TokenKind.CloseParen, x => x.Kind == TokenKind.Comma);
        if (results == null) return null;
        List<SyntaxNode> paramList = [];
        paramList.AddRange(results);
        if (!NextWith(TokenKind.CloseParen)) return null;

        // return type
        SyntaxNode? returnType = null;
        if (Try(TokenKind.Colon))
        {
            if (!Next()) return null;

            returnType = ParseTypeReference();
            if (returnType == null) return null;
        }

        // body
        List<SyntaxNode>? body = null;
        if (Try(TokenKind.OpenBrace))
        {
            if (!Next()) return null;
            results = Repeat(ParseStatement, x => x.Kind == TokenKind.CloseBrace);
            if (results == null) return null;
            body = [];
            body.AddRange(results);
            if (!NextWith(TokenKind.CloseBrace)) return null;
        }
        else
        {
            if (!NextWith(TokenKind.SemiColon)) return null;
        }

        location.MarkEnd(Reader);

        return SyntaxNode.CreateFunctionDecl(name, returnType, paramList, body, isExternal, location);
    }

    /// <summary>
    /// 関数の仮引数
    /// </summary>
    private SyntaxNode? ParseFunctionParameter()
    {
        var location = CreateLocation();
        location.MarkBegin(Reader);

        if (!Expect(TokenKind.Word)) return null;
        var name = GetTokenValue();
        if (!Next()) return null;

        SyntaxNode? paramType = null;
        if (Try(TokenKind.Colon))
        {
            if (!Next()) return null;

            paramType = ParseTypeReference();
            if (paramType == null) return null;
        }

        location.MarkEnd(Reader);

        return SyntaxNode.CreateFunctionParameter(name, paramType, location);
    }
}
