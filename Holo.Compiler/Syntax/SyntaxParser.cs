using System;
using System.IO;

namespace Holo.Compiler.Syntax;

public class Parser
{
    public UnitNode Parse(Stream stream)
    {
        var ctx = new ParserContext();
        ctx.Initialize(stream);
        ctx.Read();

        var node = new UnitNode();
        while (ctx.Kind != TokenKind.EOF)
        {
            node.Body.Add(ParseFunctionDecl(ctx));
        }

        return node;
    }

    public FuncDeclarationNode ParseFunctionDecl(ParserContext ctx)
    {
        ctx.Read();

        return new FuncDeclarationNode();
    }

    public IExpressionNode ParseExpression(ParserContext ctx)
    {
        throw new NotImplementedException();
    }
}
