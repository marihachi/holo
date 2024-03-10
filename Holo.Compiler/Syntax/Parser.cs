using System;
using System.Collections.Generic;
using System.IO;

namespace Holo.Compiler.Syntax;

public class Parser
{
    private TokenReader Reader = new TokenReader();

    private SyntaxNode? _Result;

    public bool IsSuccess => Result != null;
    public SyntaxNode Result => _Result!;
    public List<string> Errors = [];

    private void SetResult(SyntaxNode node)
    {
        _Result = node;
    }

    private void GenerateError(string message)
    {
        Errors.Add(message);
    }

    private void GenerateReadError()
    {
        GenerateError(Reader.Message);
    }

    private void GenerateUnexpectedTokenError()
    {
        GenerateError(Reader.CreateUnexpectedError());
    }

    public void Parse(Stream stream)
    {
        // clear states
        Reader.Initialize(stream);
        _Result = null;
        Errors.Clear();

        if (!Reader.Read())
        {
            GenerateReadError();
            return;
        }

        ParseUnit();
    }

    private void ParseUnit()
    {
        if (!Reader.Read())
        {
            GenerateReadError();
            return;
        }

        var beginToken = Reader.Token;

        var body = new List<SyntaxNode>();
        while (Reader.TokenKind != TokenKind.EOF)
        {
            ParseFunctionDecl();
            if (!IsSuccess) return;

            body.Add(Result!);
        }

        var endToken = Reader.Token;

        SetResult(SyntaxNode.CreateUnit(body, new NodeLocation(beginToken.Location, endToken.Location)));
    }

    private void ParseFunctionDecl()
    {
        var body = new List<SyntaxNode>();
        var beginToken = Reader.Token;

        if (Reader.TokenKind != TokenKind.Fn)
        {
            GenerateUnexpectedTokenError();
            return;
        }

        if (!Reader.Read())
        {
            GenerateReadError();
            return;
        }

        if (Reader.TokenKind != TokenKind.Identifier)
        {
            GenerateUnexpectedTokenError();
            return;
        }

        var name = (string)Reader.Token.Value!;

        if (!Reader.Read())
        {
            GenerateReadError();
            return;
        }

        // TODO: parse body

        var endToken = Reader.Token;

        SetResult(SyntaxNode.CreateFunctionDecl(name, body, new NodeLocation(beginToken.Location, endToken.Location)));
        return;
    }

    private void ParseExpression()
    {
        throw new NotImplementedException();
    }

    private void ParseStatement()
    {
        throw new NotImplementedException();
    }
}
