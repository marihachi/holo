using Holoc.Compile.Holo.Syntax.Token;

namespace Holoc.Compile.Holo.Syntax.Node;

public class NodeLocation(SyntaxToken? begin, SyntaxToken? end)
{
    public SyntaxToken? Begin = begin;
    public SyntaxToken? End = end;

    public void MarkBegin(TokenReader reader)
    {
        Begin = reader.NextToken;
    }

    public void MarkEnd(TokenReader reader)
    {
        if (Begin == null)
        {
            throw new InvalidOperationException("ノードの開始位置が設定されていません。");
        }

        End = reader.CurrentToken;
    }

    public static NodeLocation Empty => new(null, null);

    public string GetLocationString()
    {
        if (Begin == null)
        {
            throw new InvalidOperationException("ノードの開始位置が設定されていません。");
        }

        if (End == null)
        {
            throw new InvalidOperationException("ノードの終了位置が設定されていません。");
        }

        // 最初のトークンの開始位置と最後のトークンの終了位置を使って、ノード全体の位置を返します。
        return $"{Begin.BeginLocation} - {End.EndLocation}";
    }
}
