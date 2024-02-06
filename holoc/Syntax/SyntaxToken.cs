namespace holoc.Syntax;

public class SyntaxToken (
    SyntaxTokenKind kind,
    CodeLocation location,
    object? value = null
    )
{
    public SyntaxTokenKind Kind { get; set; } = kind;
    public CodeLocation Location { get; set; } = location;
    public object? Value { get; set; } = value;
}

public enum SyntaxTokenKind
{
    EOF,
    Identifier,
    NumberLiteral,

    /// <summary>"!"</summary>
    Not,
    /// <summary>"!="</summary>
    NotEq,
    /// <summary>"%"</summary>
    Percent,
    /// <summary>"%="</summary>
    PercentEq,
    /// <summary>"&"</summary>
    And,
    /// <summary>"&&"</summary>
    And2,
    /// <summary>"&="</summary>
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

    Break,
    Case,
    Continue,
    Default,
    Do,
    Else,
    External,
    Fn,
    If,
    Return,
    Switch,
    Var,
    While,
}

public class CodeLocation (
    int column,
    int line
    )
{
    public int Column { get; set; } = column;
    public int Line { get; set; } = line;
}
