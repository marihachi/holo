using System.IO;
using System.Text;

namespace holoc.Syntax;

public class SyntaxTokenReader
{
    private StreamReader SourceReader;

    private int Column = 1;
    private int Line = 1;

    public SyntaxTokenReader(Stream stream)
    {
        SourceReader = new StreamReader(stream);
    }

    public ReadTokenResult Read()
    {
        // NOTE: SourceReader.Read()したら必ず位置の更新を行う。

        while (true)
        {
            // ストリームの終わりに達していたら
            if (SourceReader.EndOfStream)
            {
                return ReadTokenResult.Succeed(
                    new SyntaxToken(SyntaxTokenKind.EOF, new CodeLocation(Column, Line))
                );
            }

            // 1文字読み取る
            var ch = (char)SourceReader.Read();

            // スペースのスキップ
            switch (ch)
            {
                case ' ':
                    Column += 1;
                    continue;
                case '\t':
                    Column += 1;
                    continue;
                case '\n':
                    Column = 1;
                    Line += 1;
                    continue;
                case '\r':
                    if (!SourceReader.EndOfStream && (char)SourceReader.Peek() == '\n')
                    {
                        SourceReader.Read();
                        Column = 1;
                        Line += 1;
                        continue;
                    }
                    return ReadTokenResult.Fail("unexpected char.");
            }

            var location = new CodeLocation(Column, Line);
             Column += 1;

            // 記号の読み取り
            switch (ch) {
                case '*':
                    if (!SourceReader.EndOfStream && (char)SourceReader.Peek() == '=')
                    {
                        SourceReader.Read();
                        Column += 1;
                        return ReadTokenResult.Succeed(
                            new SyntaxToken(SyntaxTokenKind.AsterEq, location)
                        );
                    }
                    else
                    {
                        return ReadTokenResult.Succeed(
                            new SyntaxToken(SyntaxTokenKind.Asterisk, location)
                        );
                    }
                case '+':
                    if (!SourceReader.EndOfStream && (char)SourceReader.Peek() == '=')
                    {
                        SourceReader.Read();
                        Column += 1;
                        return ReadTokenResult.Succeed(
                            new SyntaxToken(SyntaxTokenKind.PlusEq, location)
                        );
                    }
                    else
                    {
                        return ReadTokenResult.Succeed(
                            new SyntaxToken(SyntaxTokenKind.Plus, location)
                        );
                    }
                case '-':
                    if (!SourceReader.EndOfStream && (char)SourceReader.Peek() == '=')
                    {
                        SourceReader.Read();
                        Column += 1;
                        return ReadTokenResult.Succeed(
                            new SyntaxToken(SyntaxTokenKind.MinusEq, location)
                        );
                    }
                    else
                    {
                        return ReadTokenResult.Succeed(
                            new SyntaxToken(SyntaxTokenKind.Minus, location)
                        );
                    }
                case '/':
                    if (!SourceReader.EndOfStream && (char)SourceReader.Peek() == '=')
                    {
                        SourceReader.Read();
                        Column += 1;
                        return ReadTokenResult.Succeed(
                            new SyntaxToken(SyntaxTokenKind.SlashEq, location)
                        );
                    }
                    else
                    {
                        return ReadTokenResult.Succeed(
                            new SyntaxToken(SyntaxTokenKind.Slash, location)
                        );
                    }
            }

            // TODO: 識別子やキーワード、リテラルの読み取り

            return ReadTokenResult.Fail("unexpected char.");
        }
    }
}
