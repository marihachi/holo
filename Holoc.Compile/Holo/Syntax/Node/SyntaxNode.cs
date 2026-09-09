namespace Holoc.Compile.Holo.Syntax.Node;

internal interface ISyntaxNode
{
    NodeLocation Location { get; }
}

internal interface ISyntaxName
{
    string Name { get; }
}

internal interface ISyntaxNumberValue
{
    long Value { get; }
}

internal interface ISyntaxMode
{
    NodeMode Mode { get; }
}

internal static class SyntaxNodeVisitor
{
    public static void ShowSyntaxNode(ISyntaxNode? node)
    {
        if (node == null) return;

        ShowSyntaxNodeInternal(node, "", true, "");
    }

    private static void ShowSyntaxNodeInternal(ISyntaxNode node, string indent, bool isLast, string labelName)
    {
        // 現在のノードを表示
        string prefix = isLast ? "└── " : "├── ";
        Console.Write(indent + prefix);
        Console.Write(labelName.Length > 0 ? $"[{labelName}] " : "");
        Console.Write($"{node.GetType().Name}");

        // ノードの付加情報を表示
        if (node is ISyntaxName nameNode)
            Console.Write($" [Name: {nameNode.Name}]");

        if (node is ISyntaxNumberValue valueNode)
            Console.Write($" [Value: {valueNode.Value}]");

        if (node is ISyntaxMode modeNode)
            Console.Write($" [Mode: {modeNode.Mode}]");

        // ノード位置を表示
        Console.Write($" ({node.Location.GetLocationString()})");
        Console.WriteLine();

        // TODO: ここに子ノードの表示処理を追加

        //switch (node)
        //{
        //    case SyntaxUnit unit:
        //        foreach (var child in unit.Body)
        //        {
        //            Visit(child);
        //        }
        //        break;

        //    case SyntaxFunctionDecl funcDecl:
        //        if (funcDecl.ReturnType != null)
        //            Visit(funcDecl.ReturnType);
        //        funcDecl.Parameters?.ForEach(param => Visit(param));
        //        funcDecl.Body?.ForEach(stmt => Visit(stmt));
        //        break;
        //}
    }
}

internal record SyntaxNodeBase(NodeLocation Location) : ISyntaxNode { }

internal record SyntaxUnit(List<ISyntaxNode> Body, NodeLocation Location) : SyntaxNodeBase(Location) { }

internal record SyntaxModuleDecl(string Name, bool IsPartial, NodeLocation Location) : SyntaxNodeBase(Location), ISyntaxName;

internal record SyntaxFunctionDecl(string Name, ISyntaxNode? ReturnType, List<ISyntaxNode>? Parameters, List<ISyntaxNode>? Body, bool IsDeclare, bool IsExport, NodeLocation Location) : SyntaxNodeBase(Location), ISyntaxName { }

internal record SyntaxFunctionParameter(string Name, ISyntaxNode? ParamType, NodeLocation Location) : SyntaxNodeBase(Location), ISyntaxName { }

internal record SyntaxVariableDecl(string Name, ISyntaxNode? VariableType, ISyntaxNode? Initializer, bool IsDeclare, bool IsExport, NodeLocation Location) : SyntaxNodeBase(Location), ISyntaxName { }

internal record SyntaxWhileStatement(ISyntaxNode Condition, ISyntaxNode Body, NodeLocation Location) : SyntaxNodeBase(Location) { }

internal record SyntaxDoWhileStatement(ISyntaxNode Condition, ISyntaxNode Body, NodeLocation Location) : SyntaxNodeBase(Location) { }

internal record SyntaxAssignmentStatement(NodeMode Mode, ISyntaxNode Target, ISyntaxNode Expression, NodeLocation Location) : SyntaxNodeBase(Location), ISyntaxMode { }

internal record SyntaxBreakStatement(NodeLocation Location) : SyntaxNodeBase(Location);

internal record SyntaxContinueStatement(NodeLocation Location) : SyntaxNodeBase(Location);

internal record SyntaxReturnStatement(ISyntaxNode? Expression, bool IsForceReturnFunc, NodeLocation Location) : SyntaxNodeBase(Location) { }

internal record SyntaxIfExpression(ISyntaxNode Condition, ISyntaxNode ThenExpr, ISyntaxNode? ElseExpr, NodeLocation Location) : SyntaxNodeBase(Location) { }

internal record SyntaxExpressionStatement(ISyntaxNode Expression, NodeLocation Location) : SyntaxNodeBase(Location) { }

internal enum ImportMode
{
    All,
    Specific,
}

internal record SyntaxImportDeclaration(ImportMode Mode, ISyntaxNode? Members, ISyntaxNode Source, NodeLocation Location) : SyntaxNodeBase(Location) { }

internal record SyntaxNumberLiteral(long Value, NodeLocation Location) : SyntaxNodeBase(Location), ISyntaxNumberValue;

internal record SyntaxUnaryOperation(NodeMode Mode, ISyntaxNode Expression, NodeLocation Location) : SyntaxNodeBase(Location), ISyntaxMode { }

internal record SyntaxBinaryOperation(NodeMode Mode, ISyntaxNode Left, ISyntaxNode Right, NodeLocation Location) : SyntaxNodeBase(Location), ISyntaxMode { }

internal record SyntaxIfStatement(ISyntaxNode Condition, ISyntaxNode ThenStatement, ISyntaxNode? ElseStatement, NodeLocation Location) : SyntaxNodeBase(Location) { }

internal record SyntaxBlockExpression(List<ISyntaxNode> Body, NodeLocation Location) : SyntaxNodeBase(Location) { }

internal record SyntaxReference(string Name, NodeLocation Location) : SyntaxNodeBase(Location);

internal record SyntaxCall(ISyntaxNode Callee, List<ISyntaxNode> Args, NodeLocation Location) : SyntaxNodeBase(Location) { }

internal record SyntaxIndexRef(ISyntaxNode Source, ISyntaxNode Index, NodeLocation Location) : SyntaxNodeBase(Location) { }

internal record SyntaxCollectionExpression(List<ISyntaxNode> Elements, NodeLocation Location) : SyntaxNodeBase(Location) { }

internal record SyntaxNamedType(string Name, NodeLocation Location) : SyntaxNodeBase(Location);

internal record SyntaxCollectionType(ISyntaxNode? ElementType, long? Size, NodeLocation Location) : SyntaxNodeBase(Location) { }

internal record SyntaxPointerType(ISyntaxNode? ElementType, NodeLocation Location) : SyntaxNodeBase(Location) { }

// 旧クラス
public class SyntaxNode
{
    public NodeKind Kind { get; set; }
    public NodeMode Mode { get; set; } = NodeMode.None;
    public NodeLocation Location { get; set; } = NodeLocation.Empty;
    public List<SyntaxNode?>? Operands { get; set; }
    public List<SyntaxNode>? Parameters { get; set; }
    public List<SyntaxNode>? Body { get; set; }
    public string? Name { get; set; }
    public object? Value { get; set; }
    public bool IsDeclare { get; set; }
    public bool IsPartial { get; set; }
    public bool IsExport { get; set; }

    public bool IsForceReturnFunc { get; set; }

    /// <summary>
    /// SyntaxNodeのツリー構造をコンソールに表示します。
    /// </summary>
    public static void ShowSyntaxNode(SyntaxNode? node)
    {
        if (node == null)
        {
            return;
        }

        ShowSyntaxNodeInternal(node, "", true, "");
    }

    private static void ShowSyntaxNodeInternal(SyntaxNode node, string indent, bool isLast, string labelName)
    {
        // 現在のノードを表示
        string prefix = isLast ? "└── " : "├── ";
        Console.Write(indent + prefix);
        Console.Write(labelName.Length > 0 ? $"[{labelName}] " : "");
        Console.Write(node.Kind);

        // ノードの付加情報を表示
        if (!string.IsNullOrEmpty(node.Name))
        {
            Console.Write($" [Name: {node.Name}]");
        }

        if (node.Value != null)
        {
            Console.Write($" [Value: {node.Value}]");
        }

        if (node.Mode != NodeMode.None)
        {
            Console.Write($" [Mode: {node.Mode}]");
        }

        if (node.IsDeclare)
        {
            Console.Write(" [Declare]");
        }

        if (node.IsPartial)
        {
            Console.Write(" [Partial]");
        }

        if (node.IsExport)
        {
            Console.Write(" [Export]");
        }

        if (node.IsForceReturnFunc)
        {
            Console.Write(" [ForceReturnFunc]");
        }

        Console.Write($" ({node.Location.GetLocationString()})");
        Console.WriteLine();

        // インデント文字列を更新
        string nextIndent = indent + (isLast ? "    " : "│   ");

        // Operandsを表示
        if (node.Operands != null && node.Operands.Count > 0)
        {
            for (int i = 0; i < node.Operands.Count; i++)
            {
                var operand = node.Operands[i];

                if (operand != null)
                {
                    ShowSyntaxNodeInternal(operand, nextIndent, i == node.Operands.Count - 1 && (node.Parameters == null || node.Parameters.Count == 0) && (node.Body == null || node.Body.Count == 0), "O");
                }
                else
                {
                    Console.WriteLine(nextIndent + (i == node.Operands.Count - 1 && (node.Parameters == null || node.Parameters.Count == 0) && (node.Body == null || node.Body.Count == 0) ? "└── " : "├── ") + "[O] <null>");
                }
            }
        }

        // Parametersを表示
        if (node.Parameters != null && node.Parameters.Count > 0)
        {
            for (int i = 0; i < node.Parameters.Count; i++)
            {
                ShowSyntaxNodeInternal(node.Parameters[i], nextIndent, i == node.Parameters.Count - 1 && (node.Body == null || node.Body.Count == 0), "P");
            }
        }

        // Bodyを表示
        if (node.Body != null && node.Body.Count > 0)
        {
            for (int i = 0; i < node.Body.Count; i++)
            {
                ShowSyntaxNodeInternal(node.Body[i], nextIndent, i == node.Body.Count - 1, "");
            }
        }
    }
}
