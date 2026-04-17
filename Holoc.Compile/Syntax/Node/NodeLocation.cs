using Holoc.Compile.Syntax.Token;

namespace Holoc.Compile.Syntax.Node;

public class NodeLocation(SyntaxToken? begin, SyntaxToken? end)
{
    public SyntaxToken? Begin = begin;
    public SyntaxToken? End = end;

    public void MarkBegin(TokenReader reader)
    {
        Begin = reader.Token;
    }

    public void MarkEnd(TokenReader reader)
    {
        if (Begin == null)
            throw new InvalidOperationException("ノードの開始位置が設定されていません。");

        End = reader.Token;
    }

    public static NodeLocation Empty => new(null, null);

    public string GetLocationString()
    {
        if (Begin == null)
            throw new InvalidOperationException("ノードの開始位置が設定されていません。");

        if (End == null)
            throw new InvalidOperationException("ノードの終了位置が設定されていません。");

        return $"{Begin.BeginLocation} - {End.EndLocation}";
    }
}
