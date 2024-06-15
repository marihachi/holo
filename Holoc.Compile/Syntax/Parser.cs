using System;
using System.Collections.Generic;

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

    /// <summary>
    /// 新しいノード位置情報を作成します。
    /// </summary>
    private static NodeLocation CreateLocation()
    {
        return new NodeLocation(TokenLocation.Empty, TokenLocation.Empty);
    }

    /// <summary>
    /// パースエラーを生成します。
    /// </summary>
    private void GenerateError(string message)
    {
        Errors.Add(message);
    }

    /// <summary>
    /// 現在のトークンが期待する種類であるかを確認します。
    /// </summary>
    private bool Expect(TokenKind kind)
    {
        if (Reader.TokenKind == kind)
        {
            return true;
        }
        else
        {
            GenerateError(Reader.CreateUnexpectedError());
            return false;
        }
    }

    /// <summary>
    /// 次のトークンを読み進めます。
    /// </summary>
    private bool Next()
    {
        if (Reader.Read())
        {
            return true;
        }
        else
        {
            GenerateError(Reader.Error);
            return false;
        }
    }

    /// <summary>
    /// 現在のトークンが期待する種類であるかを確認し、次のトークンを読み進めます。
    /// </summary>
    private bool NextWith(TokenKind kind)
    {
        if (Reader.TokenKind != kind)
        {
            GenerateError(Reader.CreateUnexpectedError());
            return false;
        }

        if (!Reader.Read())
        {
            GenerateError(Reader.Error);
            return false;
        }

        return true;
    }

    /// <summary>
    /// 指定したパース関数を繰り返し適用します。
    /// 繰り返し完了条件に一致するまで処理は継続されます。
    /// 繰り返しの途中でパース関数がエラーを返した場合、繰り返し呼び出し全体が失敗として終了します。
    /// </summary>
    /// <param name="parseFunc">パース関数</param>
    /// <param name="termination">繰り返しの完了条件</param>
    private void Repeat(Action parseFunc, Predicate<TokenKind> termination)
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

        if (!Next()) return null;

        ParseUnit();

        return Result;
    }

    private void ParseUnit()
    {
        Result = null;

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

        if (!NextWith(TokenKind.Fn)) return;

        if (!Expect(TokenKind.Identifier)) return;

        var name = (string)Reader.Token.Value;

        if (!Next()) return;

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
