namespace holoc.Syntax;

public class ReadTokenResult(bool isSuccess, SyntaxToken? token, string? message)
{
    public bool IsSuccess { get; set; } = isSuccess;
    public SyntaxToken? Token { get; set; } = token;
    public string? Message { get; set; } = message;

    public static ReadTokenResult Succeed(SyntaxToken token)
    {
        return new ReadTokenResult(true, token, null);
    }

    public static ReadTokenResult Fail(string message)
    {
        return new ReadTokenResult(false, null, message);
    }
}
