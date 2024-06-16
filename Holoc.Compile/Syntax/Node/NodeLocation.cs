using Holoc.Compile.Syntax.Token;

namespace Holoc.Compile.Syntax.Node;

public class NodeLocation(TokenLocation begin, TokenLocation end)
{
    public TokenLocation Begin = begin;
    public TokenLocation End = end;

    public void MarkBegin(TokenReader reader)
    {
        var tokenLocation = new TokenLocation(reader.Column, reader.Line);
        Begin = tokenLocation;
    }

    public void MarkEnd(TokenReader reader)
    {
        if (Begin.Column == -1 || Begin.Line == -1)
            throw new InvalidOperationException("ノードの開始位置が設定されていません。");

        var tokenLocation = new TokenLocation(reader.Column, reader.Line);
        End = tokenLocation;
    }

    public static NodeLocation Empty => new(TokenLocation.Empty, TokenLocation.Empty);
}
