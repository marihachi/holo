using System.Collections.Generic;

namespace Holo.Compiler.Syntax;

public enum NodeKind
{
    Unit,
    FunctionDecl,
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

public enum NodeMode
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

public struct NodeLocation(TokenLocation begin, TokenLocation end)
{
    public TokenLocation Begin = begin;
    public TokenLocation End = end;
}

public class SyntaxNode
{
    public NodeKind Kind { get; set; }
    public NodeMode Mode { get; set; } = NodeMode.None;
    public NodeLocation Location { get; set; }
    public List<SyntaxNode>? Operands { get; set; }
    public List<SyntaxNode>? Body { get; set; }
    public string? Name { get; set; }
    public object? Value { get; set; }

    public static SyntaxNode CreateUnit(List<SyntaxNode> body, NodeLocation location)
    {
        return new SyntaxNode
        {
            Kind = NodeKind.Unit,
            Location = location,
            Body = body,
        };
    }

    public static SyntaxNode CreateFunctionDecl(string name, List<SyntaxNode> body, NodeLocation location)
    {
        return new SyntaxNode
        {
            Kind = NodeKind.FunctionDecl,
            Location = location,
            Name = name,
            Body = body,
        };
    }

    public static SyntaxNode CreateVariableDecl(string name, SyntaxNode? initializer, NodeLocation location)
    {
        return new SyntaxNode
        {
            Kind = NodeKind.VariableDecl,
            Location = location,
            Name = name,
            Operands = initializer != null ? [initializer] : [],
        };
    }

    public static SyntaxNode CreateWhileStatement(SyntaxNode condition, List<SyntaxNode> body, NodeLocation location)
    {
        return new SyntaxNode
        {
            Kind = NodeKind.WhileStatement,
            Location = location,
            Operands = [condition],
            Body = body,
        };
    }

    public static SyntaxNode CreateAssignStatement(NodeMode assignMode, SyntaxNode assignee, SyntaxNode expression, NodeLocation location)
    {
        return new SyntaxNode
        {
            Kind = NodeKind.AssignStatement,
            Location = location,
            Operands = [assignee, expression],
            Mode = assignMode,
        };
    }

    public static SyntaxNode CreateBreakStatement(NodeLocation location)
    {
        return new SyntaxNode
        {
            Kind = NodeKind.BreakStatement,
            Location = location,
        };
    }

    public static SyntaxNode CreateContinueStatement(NodeLocation location)
    {
        return new SyntaxNode
        {
            Kind = NodeKind.ContinueStatement,
            Location = location,
        };
    }

    public static SyntaxNode CreateReturnStatement(SyntaxNode? expression, NodeLocation location)
    {
        return new SyntaxNode
        {
            Kind = NodeKind.ReturnStatement,
            Location = location,
            Operands = expression != null ? [expression] : [],
        };
    }

    public static SyntaxNode CreateSwitchStatement(SyntaxNode condition, List<SyntaxNode> arms, NodeLocation location)
    {
        return new SyntaxNode
        {
            Kind = NodeKind.SwitchStatement,
            Location = location,
            Operands = [condition],
            Body = arms,
        };
    }

    public static SyntaxNode CreateSwitchArm(bool isDefaultArm, SyntaxNode? condition, SyntaxNode expression, NodeLocation location)
    {
        return new SyntaxNode
        {
            Kind = NodeKind.SwitchArm,
            Location = location,
            Mode = isDefaultArm ? NodeMode.DefaultArm : NodeMode.None,
            Operands = condition != null
                ? [expression, condition]
                : [expression],
        };
    }

    public static SyntaxNode CreateExpressionStatement(SyntaxNode expression, NodeLocation location)
    {
        return new SyntaxNode
        {
            Kind = NodeKind.ExpressionStatement,
            Location = location,
            Operands = [expression],
        };
    }

    public static SyntaxNode CreateNumberLiteral(int value, NodeLocation location)
    {
        return new SyntaxNode
        {
            Kind = NodeKind.NumberLiteral,
            Location = location,
            Value = value,
        };
    }

    public static SyntaxNode CreateUnaryOperation(SyntaxNode expression, NodeLocation location)
    {
        return new SyntaxNode
        {
            Kind = NodeKind.UnaryOperation,
            Location = location,
            Operands = [expression],
        };
    }

    public static SyntaxNode CreateBinaryOperation(SyntaxNode left, SyntaxNode right, NodeLocation location)
    {
        return new SyntaxNode
        {
            Kind = NodeKind.BinaryOperation,
            Location = location,
            Operands = [left, right],
        };
    }

    public static SyntaxNode CreateIf(
        SyntaxNode condition,
        SyntaxNode thenExpression,
        SyntaxNode? elseExpression,
        NodeLocation location
    )
    {
        return new SyntaxNode
        {
            Kind = NodeKind.If,
            Location = location,
            Operands = elseExpression != null
                ? [condition, thenExpression, elseExpression]
                : [condition, thenExpression],
        };
    }

    public static SyntaxNode CreateBlock(List<SyntaxNode> body, NodeLocation location)
    {
        return new SyntaxNode
        {
            Kind = NodeKind.Block,
            Location = location,
            Body = body,
        };
    }

    public static SyntaxNode CreateReference(string name, NodeLocation location)
    {
        return new SyntaxNode
        {
            Kind = NodeKind.Reference,
            Location = location,
            Name = name,
        };
    }

    public static SyntaxNode CreateCall(SyntaxNode callee, List<SyntaxNode> args, NodeLocation location)
    {
        return new SyntaxNode
        {
            Kind = NodeKind.Call,
            Location = location,
            Operands = [callee],
            Body = args,
        };
    }

    public static SyntaxNode CreateTypeReference(string name, NodeLocation location)
    {
        return new SyntaxNode
        {
            Kind = NodeKind.TypeReference,
            Location = location,
            Name = name,
        };
    }
}
