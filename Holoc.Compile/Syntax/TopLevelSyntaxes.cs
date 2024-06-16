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
        Ctx.Result = null;
        var location = ParserContext.CreateLocation();
        location.MarkBegin(Ctx.Reader);

        Ctx.Repeat(ParseFunctionDecl, x => x.Kind == TokenKind.EOF);
        if (Ctx.Results == null) return;
        List<SyntaxNode> body = [];
        body.AddRange(Ctx.Results);

        location.MarkEnd(Ctx.Reader);
        Ctx.Result = SyntaxNode.CreateUnit(body, location);
    }

    /// <summary>
    /// 関数宣言
    /// </summary>
    private void ParseFunctionDecl()
    {
        Ctx.Result = null;
        var location = ParserContext.CreateLocation();
        location.MarkBegin(Ctx.Reader);

        if (!Ctx.NextWith("fn")) return;

        // name
        if (!Ctx.Expect(TokenKind.Word)) return;
        var name = (string)Ctx.Reader.Token!.Value!;
        if (!Ctx.Next()) return;

        // parameters
        if (!Ctx.NextWith(TokenKind.OpenParen)) return;
        Ctx.Repeat(ParseFunctionParameter, x => x.Kind == TokenKind.CloseParen, x => x.Kind == TokenKind.Comma);
        if (Ctx.Results == null) return;
        List<SyntaxNode> paramList = [];
        paramList.AddRange(Ctx.Results);
        if (!Ctx.NextWith(TokenKind.CloseParen)) return;

        // body
        List<SyntaxNode>? body = null;
        if (Ctx.Try(TokenKind.OpenBrace))
        {
            if (!Ctx.Next()) return;
            Ctx.Repeat(ParseStatement, x => x.Kind == TokenKind.CloseBrace);
            if (Ctx.Results == null) return;
            body = [];
            body.AddRange(Ctx.Results);
            if (!Ctx.NextWith(TokenKind.CloseBrace)) return;
        }
        else
        {
            if (!Ctx.NextWith(TokenKind.SemiColon)) return;
        }

        location.MarkEnd(Ctx.Reader);
        Ctx.Result = SyntaxNode.CreateFunctionDecl(name, body, location);
    }

    /// <summary>
    /// 関数の仮引数
    /// </summary>
    private void ParseFunctionParameter()
    {
        Ctx.Result = null;
        var location = ParserContext.CreateLocation();
        location.MarkBegin(Ctx.Reader);

        if (!Ctx.Expect(TokenKind.Word)) return;
        var name = (string)Ctx.Reader.Token!.Value!;
        if (!Ctx.Next()) return;

        location.MarkEnd(Ctx.Reader);
        Ctx.Result = SyntaxNode.CreateFunctionParameter(name, location);
    }
}
