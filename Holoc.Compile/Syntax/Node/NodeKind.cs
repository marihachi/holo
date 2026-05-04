namespace Holoc.Compile.Syntax.Node;

public enum NodeKind : byte
{
    // Common
    Reference,
    TypeReference,
    Call,
    BlockExpression,

    // Core
    Unit,

    // Statements
    FunctionDecl,
    FunctionParameter,
    VariableDecl,
    AssignStatement,
    IfStatement,
    WhileStatement,
    //ForStatement,
    BreakStatement,
    ContinueStatement,
    ReturnStatement,
    ExpressionStatement,

    // Expressions
    WhenExpression,
    WhenArm,
    NumberLiteral,
    UnaryOperation,
    BinaryOperation,
}
