using Holoc.Compile.Syntax.Node;
using Holoc.Compile.Syntax.Token;

namespace Holoc.Compile.Syntax;

/// <summary>
/// Holo言語のLLパーサーを実装します。
/// </summary>
public partial class Parser
{
    private TokenReader Reader = new TokenReader();

    public SyntaxNode? Result;
    public List<SyntaxNode>? Results;
    public List<string> Errors = [];

    /// <summary>
    /// 新しいノード位置情報を作成します。
    /// </summary>
    private static NodeLocation CreateLocation()
    {
        return new NodeLocation(TokenLocation.Empty, TokenLocation.Empty);
    }

    private void Initialize(Stream stream)
    {
        // 状態のクリア
        Reader.Initialize(stream);
        Result = null;
        Results = null;
        Errors.Clear();

        // 最初のトークンを読み取り
        Next();
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
        if (Reader.Token.Kind != kind)
        {
            GenerateError(Reader.CreateUnexpectedError());
            return false;
        }

        return true;
    }

    /// <summary>
    /// 現在のトークンの種類を取得します。
    /// </summary>
    private TokenKind GetKind()
    {
        return Reader.Token.Kind;
    }

    /// <summary>
    /// 現在のトークンが期待する種類であるかを確認します。
    /// </summary>
    private bool Try(TokenKind kind)
    {
        if (Reader.Token.Kind != kind)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 現在のトークンが期待するキーワードであるかを確認します。
    /// </summary>
    private bool Try(string keyword)
    {
        if (Reader.Token.Kind != TokenKind.Word)
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
        if (!Reader.Read())
        {
            GenerateError(Reader.Error!);
            return false;
        }

        return true;
    }

    /// <summary>
    /// 現在のトークンが期待する種類であるかを確認し、次のトークンを読み進めます。
    /// </summary>
    private bool NextWith(TokenKind kind)
    {
        if (Reader.Token.Kind != kind)
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
        if (Reader.Token.Kind != TokenKind.Word)
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
    /// <param name="parseItem">パース関数</param>
    /// <param name="terminator">繰り返し終了のトークンかを確認する関数</param>
    /// <param name="separator">区切り文字のトークンかを確認する関数</param>
    private void Repeat(Action parseItem, Predicate<SyntaxToken> terminator, Predicate<SyntaxToken>? separator)
    {
        Results = null;

        var items = new List<SyntaxNode>();

        // 終端のトークンかを確認する
        while (!terminator(Reader.Token!))
        {
            // 2個目の項目以降は、前に区切りトークンがあることを期待する
            if (separator != null && items.Count > 0)
            {
                // 区切りトークンかを確認する
                if (!separator(Reader.Token!))
                {
                    GenerateError(Reader.CreateUnexpectedError());
                    return;
                }

                if (!Next()) return;
            }

            parseItem();
            if (Result == null) return;

            items.Add(Result);
        }

        Results = items;
    }

    public SyntaxNode? Parse(Stream stream)
    {
        Initialize(stream);
        ParseUnit();
        return Result;
    }
}
