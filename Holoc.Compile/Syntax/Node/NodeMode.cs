namespace Holoc.Compile.Syntax.Node;

public enum NodeMode : byte
{
    None,

    // math
    Add,
    Sub,
    Mul,
    Div,
    Rem,

    // bit
    ShiftLeft,
    ShiftRight,
    BitAnd,
    BitOr,
    Xor,

    // compare
    Gt,
    Lt,
    GtEq,
    LtEq,
    Eq,
    NotEq,

    // switch arm
    DefaultArm,
}
