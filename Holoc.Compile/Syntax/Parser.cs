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
    /// 現在のトークンの種類を取得します。
    /// </summary>
    private TokenKind GetKind()
    {
        return Reader.TokenKind;
    }

    /// <summary>
    /// 現在のトークンが期待する種類であるかを確認します。
    /// </summary>
    private bool Try(TokenKind kind)
    {
        if (Reader.TokenKind == kind)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 現在のトークンが期待するキーワードであるかを確認します。
    /// </summary>
    private bool Try(string keyword)
    {
        if (Reader.TokenKind != TokenKind.Word)
        {
            return false;
        }

        if ((string)Reader.Token!.Value! != keyword)
        {
            return false;
        }

        return true;
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
            GenerateError(Reader.Error!);
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
            GenerateError(Reader.Error!);
            return false;
        }

        return true;
    }

    /// <summary>
    /// 現在のトークンが期待するキーワードであるかを確認し、次のトークンを読み進めます。
    /// </summary>
    private bool NextWith(string keyword)
    {
        if (Reader.TokenKind != TokenKind.Word)
        {
            GenerateError(Reader.CreateUnexpectedError());
            return false;
        }

        if ((string)Reader.Token!.Value! != keyword)
        {
            GenerateError(Reader.CreateUnexpectedError());
            return false;
        }

        if (!Reader.Read())
        {
            GenerateError(Reader.Error!);
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
    /// <param name="terminator">繰り返し終了のトークン</param>
    private void Repeat(Action parseFunc, Predicate<SyntaxToken> terminator)
    {
        Repeat(parseFunc, terminator, null);
    }

    /// <summary>
    /// 指定したパース関数を繰り返し適用します。
    /// 繰り返し完了条件に一致するまで処理は継続されます。
    /// 繰り返しの途中でパース関数がエラーを返した場合、繰り返し呼び出し全体が失敗として終了します。
    /// </summary>
    /// <param name="parseFunc">パース関数</param>
    /// <param name="terminator">繰り返し終了のトークン</param>
    /// <param name="separator">区切り文字のトークン</param>
    private void Repeat(Action parseFunc, Predicate<SyntaxToken> terminator, Predicate<SyntaxToken>? separator)
    {
        Results = null;

        var items = new List<SyntaxNode>();
        while (!terminator(Reader.Token!))
        {
            if (separator != null && items.Count > 0)
            {
                if (!separator(Reader.Token!))
                {
                    GenerateError(Reader.CreateUnexpectedError());
                    return;
                }
                if (!Next()) return;
            }
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

        Repeat(ParseFunctionDecl, x => x.Kind == TokenKind.EOF);
        if (Results == null) return;
        List<SyntaxNode> body = [];
        body.AddRange(Results);

        location.MarkEnd(Reader);
        Result = SyntaxNode.CreateUnit(body, location);
    }

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

    private void ParseExpression()
    {
        throw new NotImplementedException();
    }

    private void ParseStatement()
    {
        Result = null;
        var location = CreateLocation();
        location.MarkBegin(Reader);

        if (Try("return"))
        {
            if (!Next()) return;
            if (!NextWith(TokenKind.SemiColon)) return;
            location.MarkEnd(Reader);
            Result = SyntaxNode.CreateReturnStatement(null, location);
            return;
        }

        // TODO: statements
        GenerateError(Reader.CreateUnexpectedError());
    }
}
