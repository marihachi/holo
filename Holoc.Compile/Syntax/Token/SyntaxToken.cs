namespace Holoc.Compile.Syntax.Token;

public enum TokenKind : byte
{
    EOF,
    Word,
    NumberLiteral,

    /// <summary>"!"</summary>
    Not,
    /// <summary>"!="</summary>
    NotEq,
    /// <summary>"%"</summary>
    Percent,
    /// <summary>"%="</summary>
    PercentEq,
    /// <summary>"&amp;"</summary>
    And,
    /// <summary>"&amp;&amp;"</summary>
    And2,
    /// <summary>"&amp;="</summary>
    AndEq,
    /// <summary>"("</summary>
    OpenParen,
    /// <summary>")"</summary>
    CloseParen,
    /// <summary>"*"</summary>
    Asterisk,
    /// <summary>"*="</summary>
    AsterEq,
    /// <summary>"+"</summary>
    Plus,
    /// <summary>"+="</summary>
    PlusEq,
    /// <summary>","</summary>
    Comma,
    /// <summary>"-"</summary>
    Minus,
    /// <summary>"-="</summary>
    MinusEq,
    /// <summary>"/"</summary>
    Slash,
    /// <summary>"/="</summary>
    SlashEq,
    /// <summary>":"</summary>
    Colon,
    /// <summary>";"</summary>
    SemiColon,
    /// <summary>"<"</summary>
    Lt,
    /// <summary>"<="</summary>
    LtEq,
    /// <summary>"<<"</summary>
    Lt2,
    /// <summary>"<<="</summary>
    Lt2Eq,
    /// <summary>"="</summary>
    Eq,
    /// <summary>"=="</summary>
    Eq2,
    /// <summary>">"</summary>
    Gt,
    /// <summary>">="</summary>
    GtEq,
    /// <summary>">>"</summary>
    Gt2,
    /// <summary>">>="</summary>
    Gt2Eq,
    /// <summary>"^"</summary>
    Hat,
    /// <summary>"^="</summary>
    HatEq,
    /// <summary>"["</summary>
    OpenBracket,
    /// <summary>"]"</summary>
    CloseBracket,
    /// <summary>"{"</summary>
    OpenBrace,
    /// <summary>"|"</summary>
    Or,
    /// <summary>"||"</summary>
    Or2,
    /// <summary>"|="</summary>
    OrEq,
    /// <summary>"}"</summary>
    CloseBrace,

    //Break,
    //Case,
    //Continue,
    //Default,
    //Do,
    //Else,
    //External,
    //Fn,
    //If,
    //Return,
    //Switch,
    //Var,
    //While,
}

public struct TokenLocation(long column, long line)
{
    public long Column = column;
    public long Line = line;

    public static TokenLocation Empty => new(-1, -1);
}

public class SyntaxToken(TokenKind kind, TokenLocation location, object? value = null)
{
    public TokenKind Kind { get; set; } = kind;
    public TokenLocation Location { get; set; } = location;
    public object? Value { get; set; } = value;
}
