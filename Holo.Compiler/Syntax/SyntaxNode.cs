using System.Collections.Generic;

namespace Holo.Compiler.Syntax;

public enum NodeKind
{
    Unit,
    FunctionDecl,
    VariableDecl,
    While,
    NumberLiteral,
    UnaryOperation,
    BinaryOperation,
    IfExpression,
    BlockExpression,
    ReferenceExpression,
    TypeReference,
}

public struct NodeLocation(TokenLocation begin, TokenLocation end)
{
    public TokenLocation Begin = begin;
    public TokenLocation End = end;
}

public class SyntaxNode
{
    public NodeKind Kind { get; set; }
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

    public static SyntaxNode CreateWhile(SyntaxNode condition, List<SyntaxNode> body, NodeLocation location)
    {
        return new SyntaxNode
        {
            Kind = NodeKind.While,
            Location = location,
            Operands = [condition],
            Body = body,
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

    public static SyntaxNode CreateIfExpression(
        SyntaxNode condition,
        SyntaxNode thenExpression,
        SyntaxNode? elseExpression,
        NodeLocation location
    )
    {
        return new SyntaxNode
        {
            Kind = NodeKind.IfExpression,
            Location = location,
            Operands = elseExpression != null
                ? [condition, thenExpression, elseExpression]
                : [condition, thenExpression],
        };
    }

    public static SyntaxNode CreateBlockExpression(List<SyntaxNode> body, NodeLocation location)
    {
        return new SyntaxNode
        {
            Kind = NodeKind.BlockExpression,
            Location = location,
            Body = body,
        };
    }

    public static SyntaxNode CreateReferenceExpression(string name, NodeLocation location)
    {
        return new SyntaxNode
        {
            Kind = NodeKind.ReferenceExpression,
            Location = location,
            Name = name,
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
