using System;
using System.Collections.Generic;
using System.IO;

namespace Holoc.Compile.Syntax;

/// <summary>
/// Holo言語のLLパーサーを実装します。
/// </summary>
public class Parser
{
    private TokenReader Reader = new TokenReader();

    private SyntaxNode? Result;
    private List<SyntaxNode>? Results;
    public List<string> Errors = [];

    private static NodeLocation CreateLocation()
    {
        return new NodeLocation(TokenLocation.Empty, TokenLocation.Empty);
    }

    private void GenerateError(string message)
    {
        Errors.Add(message);
    }

    private void GenerateReadError()
    {
        GenerateError(Reader.Error);
    }

    private void GenerateUnexpectedTokenError()
    {
        GenerateError(Reader.CreateUnexpectedError());
    }

    /// <summary>
    /// 指定したパース関数を繰り返し適用します。
    /// 繰り返し完了条件に一致するまで処理は継続されます。
    /// 繰り返しの途中でパース関数がエラーを返した場合、繰り返し呼び出し全体が失敗として終了します。
    /// </summary>
    /// <param name="parseFunc">パース関数</param>
    /// <param name="termination">繰り返しの完了条件</param>
    public void Repeat(Action parseFunc, Predicate<TokenKind> termination)
    {
        Results = null;

        var items = new List<SyntaxNode>();
        while (!termination(Reader.TokenKind))
        {
            parseFunc();
            if (Result == null) return;
            items.Add(Result);
        }

        Results = items;
    }

    public SyntaxNode? Parse(Stream stream)
    {
        // clear states
        Reader.Initialize(stream);
        Result = null;
        Results = null;
        Errors.Clear();

        ParseUnit();

        return Result;
    }

    private void ParseUnit()
    {
        Result = null;

        if (!Reader.Read())
        {
            GenerateReadError();
            return;
        }

        var location = CreateLocation();

        location.MarkBegin(Reader);

        Repeat(ParseFunctionDecl, k => k == TokenKind.EOF);
        if (Results == null) return;
        var body = Results;

        location.MarkEnd(Reader);

        Result = SyntaxNode.CreateUnit(body, location);
    }

    private void ParseFunctionDecl()
    {
        Result = null;

        var location = CreateLocation();

        location.MarkBegin(Reader);

        // expect "fn"
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

        // expect ident
        if (Reader.TokenKind != TokenKind.Identifier)
        {
            GenerateUnexpectedTokenError();
            return;
        }

        var name = (string)Reader.Token.Value;

        if (!Reader.Read())
        {
            GenerateReadError();
            return;
        }

        // TODO: parse body
        var body = new List<SyntaxNode>();

        location.MarkEnd(Reader);

        Result = SyntaxNode.CreateFunctionDecl(name, body, location);
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
