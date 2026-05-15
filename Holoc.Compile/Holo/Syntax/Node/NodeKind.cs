namespace Holoc.Compile.Holo.Syntax.Node;

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
    FunctionDeclaration,
    FunctionParameter,
    VariableDeclaration,
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
    NumberLiteral, // value type: long
    UnaryOperation,
    BinaryOperation,
}
