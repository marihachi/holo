namespace Holoc.Compile.Holo.Syntax.Node;

public enum NodeKind : byte
{
    // Common
    NamedType,
    CollectionType,

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
    Reference,
    NumberLiteral, // value type: long
    Call,
    IndexRef,
    BlockExpression,
    CollectionExpression,
    WhenExpression,
    WhenArm,
    UnaryOperation,
    BinaryOperation,
}
