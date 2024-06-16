using Holoc.Compile.Syntax.Node;
using Holoc.Compile.Syntax.Token;

namespace Holoc.Compile.Syntax;

public partial class Parser
{
    /// <summary>
    /// コンパイル単位
    /// </summary>
    private void ParseUnit()
    {
        Result = null;
        var location = CreateLocation();
        location.MarkBegin(Reader);

        Repeat(ParseFunctionDecl, x => x.Kind == TokenKind.EOF);
        if (Results == null) return;
        List<SyntaxNode> body = [];
        body.AddRange(Results);

        location.MarkEnd(Reader);
        Result = SyntaxNode.CreateUnit(body, location);
    }

    /// <summary>
    /// 関数宣言
    /// </summary>
    private void ParseFunctionDecl()
    {
        Result = null;
        var location = CreateLocation();
        location.MarkBegin(Reader);

        if (!NextWith("fn")) return;

        // name
        if (!Expect(TokenKind.Word)) return;
        var name = (string)Reader.Token!.Value!;
        if (!Next()) return;

        // parameters
        if (!NextWith(TokenKind.OpenParen)) return;
        Repeat(ParseFunctionParameter, x => x.Kind == TokenKind.CloseParen, x => x.Kind == TokenKind.Comma);
        if (Results == null) return;
        List<SyntaxNode> paramList = [];
        paramList.AddRange(Results);
        if (!NextWith(TokenKind.CloseParen)) return;

        // body
        List<SyntaxNode>? body = null;
        if (Try(TokenKind.OpenBrace))
        {
            if (!Next()) return;
            Repeat(ParseStatement, x => x.Kind == TokenKind.CloseBrace);
            if (Results == null) return;
            body = [];
            body.AddRange(Results);
            if (!NextWith(TokenKind.CloseBrace)) return;
        }
        else
        {
            if (!NextWith(TokenKind.SemiColon)) return;
        }

        location.MarkEnd(Reader);
        Result = SyntaxNode.CreateFunctionDecl(name, body, location);
    }

    /// <summary>
    /// 関数の仮引数
    /// </summary>
    private void ParseFunctionParameter()
    {
        Result = null;
        var location = CreateLocation();
        location.MarkBegin(Reader);

        if (!Expect(TokenKind.Word)) return;
        var name = (string)Reader.Token!.Value!;
        if (!Next()) return;

        location.MarkEnd(Reader);
        Result = SyntaxNode.CreateFunctionParameter(name, location);
    }
}
