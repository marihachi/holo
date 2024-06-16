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
        if (Ctx.Try("return"))
        {
            ParseReturn();
            return;
        }

        Ctx.GenerateError(Ctx.Reader.CreateUnexpectedError());
    }

    /// <summary>
    /// return文
    /// </summary>
    private void ParseReturn()
    {
        Ctx.Result = null;
        var location = ParserContext.CreateLocation();
        location.MarkBegin(Ctx.Reader);

        if (!Ctx.NextWith("return")) return;
        if (!Ctx.NextWith(TokenKind.SemiColon)) return;

        location.MarkEnd(Ctx.Reader);
        Ctx.Result = SyntaxNode.CreateReturnStatement(null, location);
    }
}
