using System;
using System.Collections.Generic;
using System.IO;

namespace Holo.Compiler.Syntax;

public class Parser
{
    public SyntaxNode Parse(Stream stream)
    {
        var ctx = new ParserContext();
        ctx.Initialize(stream);
        ctx.Read();

        var beginToken = ctx.Token;

        var body = new List<SyntaxNode>();

        while (ctx.Kind != TokenKind.EOF)
        {
            body.Add(ParseFunctionDecl(ctx));
        }

        var endToken = ctx.Token;

        return SyntaxNode.CreateUnit(body, new NodeLocation(beginToken!.Location, endToken!.Location));
    }

    public SyntaxNode ParseFunctionDecl(ParserContext ctx)
    {
        var beginToken = ctx.Token;

        var body = new List<SyntaxNode>();

        var endToken = ctx.Token;

        return SyntaxNode.CreateFunctionDecl("f", body, new NodeLocation(beginToken!.Location, endToken!.Location));
    }

    public SyntaxNode ParseExpression(ParserContext ctx)
    {
        throw new NotImplementedException();
    }
}
