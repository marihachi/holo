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
        if (Try("return"))
        {
            ParseReturn();
            return;
        }

        GenerateError(Reader.CreateUnexpectedError());
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
}
