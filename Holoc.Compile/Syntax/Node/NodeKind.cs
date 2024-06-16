namespace Holoc.Compile.Syntax.Node;

public enum NodeKind : byte
{
    Unit,
    FunctionDecl,
    FunctionParameter,
    VariableDecl,
    WhileStatement,
    AssignStatement,
    BreakStatement,
    ContinueStatement,
    ReturnStatement,
    SwitchStatement,
    SwitchArm,
    ExpressionStatement,
    NumberLiteral,
    UnaryOperation,
    BinaryOperation,
    If,
    Block,
    Reference,
    Call,
    TypeReference,
}
