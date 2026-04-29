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
    SwitchExpression,
    SwitchArm,
    ExpressionStatement,
    NumberLiteral,
    UnaryOperation,
    BinaryOperation,
    IfExpression,
    BlockExpression,
    Reference,
    Call,
    TypeReference,
}
