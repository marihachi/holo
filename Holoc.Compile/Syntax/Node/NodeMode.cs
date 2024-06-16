namespace Holoc.Compile.Syntax.Node;

public enum NodeMode : byte
{
    None,

    // assign
    Add,
    Sub,
    Mul,
    Div,
    Rem,
    ShiftLeft,
    ShiftRight,
    BitAnd,
    BitOr,
    Xor,

    // switch arm
    DefaultArm,
}
