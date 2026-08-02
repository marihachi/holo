using Holoc.Compile.Holo.Syntax.Node;
using Holoc.Compile.Holo.Syntax.Token;

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
        var isDeclare = false;
        var isPartial = false;
        var isExport = false;

        // 修飾子は順不同
        while (true)
        {
            if (Try("declare"))
            {
                Next();
                isDeclare = true;
                continue;
            }

            if (Try("partial"))
            {
                Next();
                isPartial = true;
                continue;
            }

            if (Try("export"))
            {
                Next();
                isExport = true;
                continue;
            }

            break;
        }

        // declareは変数か関数の宣言にのみ指定可能
        if (isDeclare && !Try("fn", "var"))
        {
            GenerateError(Reader.CreateUnexpectedError());
            return null;
        }

        // partialはmodule宣言にのみ指定可能
        if (isPartial && !Try("module"))
        {
            GenerateError(Reader.CreateUnexpectedError());
            return null;
        }

        // exportは変数か関数の宣言にのみ指定可能
        if (isExport && !Try("fn", "var"))
        {
            GenerateError(Reader.CreateUnexpectedError());
            return null;
        }

        if (Try("module"))
        {
            return ParseModuleDecl(isPartial);
        }

        if (Try("fn"))
        {
            return ParseFunctionDecl(isDeclare, isExport);
        }

        if (Try("var"))
        {
            return ParseVariableDeclaration(isDeclare, isExport);
        }

        GenerateError(Reader.CreateUnexpectedError());
        return null;
    }

    private SyntaxNode? ParseModuleDecl(bool isPartial)
    {
        var location = CreateLocation();
        location.MarkBegin(Reader);

        if (!Next()) return null;

        // name
        // TODO: ドット区切りの名前空間を指定可能にする
        if (!Expect(TokenKind.Word)) return null;
        var name = GetTokenValue();
        if (!Next()) return null;

        location.MarkEnd(Reader);

        return SyntaxNode.CreateModuleDecl(name, isPartial, location);
    }

    /// <summary>
    /// 関数宣言
    /// </summary>
    private SyntaxNode? ParseFunctionDecl(bool isDeclare, bool isExport)
    {
        List<SyntaxNode>? results;

        var location = CreateLocation();
        location.MarkBegin(Reader);

        if (!Next()) return null;

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

            returnType = ParseType();
            if (returnType == null) return null;
        }

        // body
        List<SyntaxNode>? body = null;
        if (Try(TokenKind.OpenBrace))
        {
            body = ParseBlock();
            if (body == null) return null;
        }
        else
        {
            if (!NextWith(TokenKind.SemiColon)) return null;
        }

        location.MarkEnd(Reader);

        return SyntaxNode.CreateFunctionDecl(name, returnType, paramList, body, isDeclare, isExport, location);
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

            paramType = ParseType();
            if (paramType == null) return null;
        }

        location.MarkEnd(Reader);

        return SyntaxNode.CreateFunctionParameter(name, paramType, location);
    }
}
