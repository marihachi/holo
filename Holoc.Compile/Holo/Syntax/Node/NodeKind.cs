namespace Holoc.Compile.Holo.Syntax.Node;

public enum NodeKind : byte
{
    // Common
    NamedType,
    CollectionType,
    PointerType,

    // Core
    Unit,
    ModuleDeclaration,
    ImportDeclaration,

    // Statements
    FunctionDeclaration,
    FunctionParameter,
    VariableDeclaration,
    AssignmentStatement,
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
    IfExpression,
    UnaryOperation,
    BinaryOperation,
}
