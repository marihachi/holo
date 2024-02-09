using System.Collections.Generic;

namespace Holo.Compiler.Syntax;

public struct NodeLocation(TokenLocation begin, TokenLocation end)
{
    public TokenLocation Begin = begin;
    public TokenLocation End = end;
}

public interface ISyntaxNode
{
    NodeLocation Location { get; set; }
}

public interface ITopLevelNode {}
public interface IStepNode {}
public interface ITypeNode {}

public interface IStatementNode {}
public interface IExpressionNode {}

public interface IDeclarationNode
{
    string Name { get; set; }
}

public class UnitNode(
    List<ITopLevelNode> body,
    NodeLocation location
) : ISyntaxNode
{
    public List<ITopLevelNode> Body { get; } = body;
    public NodeLocation Location { get; set; } = location;
}

public class FuncDeclarationNode(
    string name,
    List<IStepNode> body,
    NodeLocation location
) : ISyntaxNode, ITopLevelNode, IDeclarationNode
{
    public string Name { get; set; } = name;
    public List<IStepNode> Body { get; } = body;
    public NodeLocation Location { get; set; } = location;
}

public class VariableDeclNode(
    string name,
    IExpressionNode? initializer,
    NodeLocation location
) : ISyntaxNode, IStepNode, IDeclarationNode
{
    public string Name { get; set; } = name;
    public IExpressionNode? Initializer { get; set; } = initializer;
    public NodeLocation Location { get; set; } = location;
}

public class WhileNode(
    IExpressionNode condition,
    List<IStepNode> body,
    NodeLocation location
) : ISyntaxNode, IStepNode, IStatementNode
{
    public IExpressionNode Condition { get; set; } = condition;
    public List<IStepNode> Body { get; } = body;
    public NodeLocation Location { get; set; } = location;
}

public class NumberLiteralNode(
    NodeLocation location
) : ISyntaxNode, IStepNode, IExpressionNode
{
    public NodeLocation Location { get; set; } = location;
}

public class UnaryOperationNode(
    IExpressionNode expression,
    NodeLocation location
) : ISyntaxNode, IStepNode, IExpressionNode
{
    public IExpressionNode Expression { get; set; } = expression;
    public NodeLocation Location { get; set; } = location;
}

public class BinaryOperationNode(
    IExpressionNode leftExpression,
    IExpressionNode rightExpression,
    NodeLocation location
) : ISyntaxNode, IStepNode, IExpressionNode
{
    public IExpressionNode LeftExpression { get; set; } = leftExpression;
    public IExpressionNode RightExpression { get; set; } = rightExpression;
    public NodeLocation Location { get; set; } = location;
}

public class IfExpressionNode(
    IExpressionNode conditionExpression,
    IStepNode thenExpression,
    IStepNode? elseExpression,
    NodeLocation location
) : ISyntaxNode, IStepNode, IExpressionNode
{
    public IExpressionNode ConditionExpression { get; set; } = conditionExpression;
    public IStepNode ThenExpression { get; } = thenExpression;
    public IStepNode? ElseExpression { get; } = elseExpression;
    public NodeLocation Location { get; set; } = location;
}

public class BlockExpressionNode(
    List<IStepNode> body,
    NodeLocation location
) : ISyntaxNode, IStepNode, IExpressionNode
{
    public List<IStepNode> Body { get; } = body;
    public NodeLocation Location { get; set; } = location;
}

public class ReferenceExpressionNode(
    string name,
    NodeLocation location
) : ISyntaxNode, IStepNode, IExpressionNode
{
    public string Name { get; set; } = name;
    public NodeLocation Location { get; set; } = location;
}

public class TypeReferenceNode(
    string name,
    NodeLocation location
) : ISyntaxNode, ITypeNode
{
    public string Name { get; set; } = name;
    public NodeLocation Location { get; set; } = location;
}
