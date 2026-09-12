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

internal interface ISyntaxImportMode
{
    ImportMode Mode { get; }
}

internal interface ISyntaxCollectionSize
{
    long? Size { get; }
}

internal interface ISyntaxDeclare
{
    bool IsDeclare { get; }
}

internal interface ISyntaxExport
{
    bool IsExport { get; }
}

internal interface ISyntaxPartial
{
    bool IsPartial { get; }
}

internal interface ISyntaxForceReturn
{
    bool IsForceReturnFunc { get; }
}

/// <summary>
/// ASTを階層表示します。
/// </summary>
internal static class SyntaxView
{
    /// <summary>
    /// 階層表示する上での子ノードを表します。
    /// 単一または複数の子ノードを保持します。
    /// </summary>
    private class SyntaxViewChild
    {
        public string Label { get; init; } = "";
        public ISyntaxNode? Node { get; init; }
        public List<ISyntaxNode>? Nodes { get; init; }
        public bool IsList { get; init; }
    }

    public static void Show(ISyntaxNode? node)
    {
        if (node == null) return;

        ShowNode(node, "", true, "");
    }

    private static void ShowNode(ISyntaxNode node, string indent, bool isLast, string labelName)
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

        if (node is ISyntaxImportMode importModeNode)
            Console.Write($" [ImportMode: {importModeNode.Mode}]");

        if (node is ISyntaxCollectionSize sizeNode && sizeNode.Size != null)
            Console.Write($" [Size: {sizeNode.Size}]");

        if (node is ISyntaxDeclare declareNode && declareNode.IsDeclare)
            Console.Write(" [Declare]");

        if (node is ISyntaxExport exportNode && exportNode.IsExport)
            Console.Write(" [Export]");

        if (node is ISyntaxPartial partialNode && partialNode.IsPartial)
            Console.Write(" [Partial]");

        if (node is ISyntaxForceReturn forceReturnNode && forceReturnNode.IsForceReturnFunc)
            Console.Write(" [ForceReturnFunc]");

        // ノード位置を表示
        Console.Write($" ({node.Location.GetLocationString()})");
        Console.WriteLine();

        // 子ノードを表示
        var children = CreateChildren(node);
        string nextIndent = indent + (isLast ? "    " : "│   ");
        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];
            bool isLastChild = i == children.Count - 1;
            string childPrefix = isLastChild ? "└── " : "├── ";

            if (child.IsList)
            {
                if (child.Nodes == null)
                {
                    Console.WriteLine($"{nextIndent}{childPrefix}[{child.Label}] <null>");
                }
                else if (child.Nodes.Count == 0)
                {
                    Console.WriteLine($"{nextIndent}{childPrefix}[{child.Label}] <empty>");
                }
                else
                {
                    // リストは見出し行を出し、その下に要素をぶら下げる
                    Console.WriteLine($"{nextIndent}{childPrefix}[{child.Label}]");
                    string listIndent = nextIndent + (isLastChild ? "    " : "│   ");
                    for (int j = 0; j < child.Nodes.Count; j++)
                    {
                        ShowNode(child.Nodes[j], listIndent, j == child.Nodes.Count - 1, "");
                    }
                }
            }
            else
            {
                if (child.Node == null)
                {
                    Console.WriteLine($"{nextIndent}{childPrefix}[{child.Label}] <null>");
                }
                else
                {
                    ShowNode(child.Node, nextIndent, isLastChild, child.Label);
                }
            }
        }
    }

    /// <summary>
    /// ノードの子ノードを表示順に列挙します。
    /// </summary>
    private static List<SyntaxViewChild> CreateChildren(ISyntaxNode node)
    {
        List<SyntaxViewChild> children;
        switch (node)
        {
            case SyntaxUnit unit:
                children = [CreateChildList("Body", unit.Body)];
                break;

            case SyntaxFunctionDecl funcDecl:
                children =
                [
                    CreateChild("ReturnType", funcDecl.ReturnType),
                    CreateChildList("Parameters", funcDecl.Parameters),
                    CreateChildList("Body", funcDecl.Body),
                ];
                break;

            case SyntaxFunctionParameter funcParam:
                children = [CreateChild("ParamType", funcParam.ParamType)];
                break;

            case SyntaxVariableDecl varDecl:
                children =
                [
                    CreateChild("VariableType", varDecl.VariableType),
                    CreateChild("Initializer", varDecl.Initializer),
                ];
                break;

            case SyntaxWhileStatement whileStatement:
                children =
                [
                    CreateChild("Condition", whileStatement.Condition),
                    CreateChild("Body", whileStatement.Body),
                ];
                break;

            case SyntaxDoWhileStatement doWhileStatement:
                children =
                [
                    CreateChild("Condition", doWhileStatement.Condition),
                    CreateChild("Body", doWhileStatement.Body),
                ];
                break;

            case SyntaxAssignmentStatement assignment:
                children =
                [
                    CreateChild("Target", assignment.Target),
                    CreateChild("Expression", assignment.Expression),
                ];
                break;

            case SyntaxReturnStatement returnStatement:
                children = [CreateChild("Expression", returnStatement.Expression)];
                break;

            case SyntaxIfExpression ifExpression:
                children =
                [
                    CreateChild("Condition", ifExpression.Condition),
                    CreateChild("ThenExpr", ifExpression.ThenExpr),
                    CreateChild("ElseExpr", ifExpression.ElseExpr),
                ];
                break;

            case SyntaxIfStatement ifStatement:
                children =
                [
                    CreateChild("Condition", ifStatement.Condition),
                    CreateChild("ThenStatement", ifStatement.ThenStatement),
                    CreateChild("ElseStatement", ifStatement.ElseStatement),
                ];
                break;

            case SyntaxExpressionStatement expressionStatement:
                children = [CreateChild("Expression", expressionStatement.Expression)];
                break;

            case SyntaxImportDeclaration importDecl:
                children =
                [
                    CreateChild("Members", importDecl.Members),
                    CreateChild("Source", importDecl.Source),
                ];
                break;

            case SyntaxUnaryOperation unaryOperation:
                children = [CreateChild("Expression", unaryOperation.Expression)];
                break;

            case SyntaxBinaryOperation binaryOperation:
                children =
                [
                    CreateChild("Left", binaryOperation.Left),
                    CreateChild("Right", binaryOperation.Right),
                ];
                break;

            case SyntaxBlockExpression blockExpression:
                children = [CreateChildList("Body", blockExpression.Body)];
                break;

            case SyntaxCall call:
                children =
                [
                    CreateChild("Callee", call.Callee),
                    CreateChildList("Args", call.Args),
                ];
                break;

            case SyntaxIndexRef indexRef:
                children =
                [
                    CreateChild("Source", indexRef.Source),
                    CreateChild("Index", indexRef.Index),
                ];
                break;

            case SyntaxCollectionExpression collectionExpression:
                children = [CreateChildList("Elements", collectionExpression.Elements)];
                break;

            case SyntaxCollectionType collectionType:
                children = [CreateChild("ElementType", collectionType.ElementType)];
                break;

            case SyntaxPointerType pointerType:
                children = [CreateChild("ElementType", pointerType.ElementType)];
                break;

            default:
                // 子ノードを持たないノード
                children = [];
                break;
        }

        return children;
    }

    private static SyntaxViewChild CreateChild(string label, ISyntaxNode? node)
    {
        return new SyntaxViewChild { Label = label, Node = node, IsList = false };
    }

    private static SyntaxViewChild CreateChildList(string label, List<ISyntaxNode>? nodes)
    {
        return new SyntaxViewChild { Label = label, Nodes = nodes, IsList = true };
    }
}

internal record SyntaxNodeBase(NodeLocation Location) : ISyntaxNode { }

internal record SyntaxUnit(List<ISyntaxNode> Body, NodeLocation Location) : SyntaxNodeBase(Location) { }

internal record SyntaxModuleDecl(string Name, bool IsPartial, NodeLocation Location) : SyntaxNodeBase(Location), ISyntaxName, ISyntaxPartial;

internal record SyntaxFunctionDecl(string Name, ISyntaxNode? ReturnType, List<ISyntaxNode>? Parameters, List<ISyntaxNode>? Body, bool IsDeclare, bool IsExport, NodeLocation Location) : SyntaxNodeBase(Location), ISyntaxName, ISyntaxDeclare, ISyntaxExport { }

internal record SyntaxFunctionParameter(string Name, ISyntaxNode? ParamType, NodeLocation Location) : SyntaxNodeBase(Location), ISyntaxName { }

internal record SyntaxVariableDecl(string Name, ISyntaxNode? VariableType, ISyntaxNode? Initializer, bool IsDeclare, bool IsExport, NodeLocation Location) : SyntaxNodeBase(Location), ISyntaxName, ISyntaxDeclare, ISyntaxExport { }

internal record SyntaxWhileStatement(ISyntaxNode Condition, ISyntaxNode Body, NodeLocation Location) : SyntaxNodeBase(Location) { }

internal record SyntaxDoWhileStatement(ISyntaxNode Condition, ISyntaxNode Body, NodeLocation Location) : SyntaxNodeBase(Location) { }

internal record SyntaxAssignmentStatement(NodeMode Mode, ISyntaxNode Target, ISyntaxNode Expression, NodeLocation Location) : SyntaxNodeBase(Location), ISyntaxMode { }

internal record SyntaxBreakStatement(NodeLocation Location) : SyntaxNodeBase(Location);

internal record SyntaxContinueStatement(NodeLocation Location) : SyntaxNodeBase(Location);

internal record SyntaxReturnStatement(ISyntaxNode? Expression, bool IsForceReturnFunc, NodeLocation Location) : SyntaxNodeBase(Location), ISyntaxForceReturn { }

internal record SyntaxIfExpression(ISyntaxNode Condition, ISyntaxNode ThenExpr, ISyntaxNode? ElseExpr, NodeLocation Location) : SyntaxNodeBase(Location) { }

internal record SyntaxExpressionStatement(ISyntaxNode Expression, NodeLocation Location) : SyntaxNodeBase(Location) { }

internal enum ImportMode
{
    All,
    Specific,
}

internal record SyntaxImportDeclaration(ImportMode Mode, ISyntaxNode? Members, ISyntaxNode Source, NodeLocation Location) : SyntaxNodeBase(Location), ISyntaxImportMode { }

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

internal record SyntaxCollectionType(ISyntaxNode? ElementType, long? Size, NodeLocation Location) : SyntaxNodeBase(Location), ISyntaxCollectionSize { }

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
