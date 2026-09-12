namespace Holoc.Compile.Holo.Syntax.Node;

public interface ISyntaxNode
{
    NodeLocation Location { get; }
}

public interface ISyntaxName
{
    string Name { get; }
}

public interface ISyntaxNumberValue
{
    long Value { get; }
}

public interface ISyntaxMode
{
    NodeMode Mode { get; }
}

public interface ISyntaxImportMode
{
    ImportMode Mode { get; }
}

public interface ISyntaxCollectionSize
{
    long? Size { get; }
}

public interface ISyntaxDeclare
{
    bool IsDeclare { get; }
}

public interface ISyntaxExport
{
    bool IsExport { get; }
}

public interface ISyntaxPartial
{
    bool IsPartial { get; }
}

public interface ISyntaxForceReturn
{
    bool IsForceReturnFunc { get; }
}

/// <summary>
/// ASTを階層表示します。
/// </summary>
public static class SyntaxView
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

public record SyntaxNodeBase(NodeLocation Location) : ISyntaxNode { }

public record SyntaxUnit(List<ISyntaxNode> Body, NodeLocation Location) : SyntaxNodeBase(Location) { }

public record SyntaxModuleDecl(string Name, bool IsPartial, NodeLocation Location) : SyntaxNodeBase(Location), ISyntaxName, ISyntaxPartial;

public record SyntaxFunctionDecl(string Name, ISyntaxNode? ReturnType, List<ISyntaxNode>? Parameters, List<ISyntaxNode>? Body, bool IsDeclare, bool IsExport, NodeLocation Location) : SyntaxNodeBase(Location), ISyntaxName, ISyntaxDeclare, ISyntaxExport { }

public record SyntaxFunctionParameter(string Name, ISyntaxNode? ParamType, NodeLocation Location) : SyntaxNodeBase(Location), ISyntaxName { }

public record SyntaxVariableDecl(string Name, ISyntaxNode? VariableType, ISyntaxNode? Initializer, bool IsDeclare, bool IsExport, NodeLocation Location) : SyntaxNodeBase(Location), ISyntaxName, ISyntaxDeclare, ISyntaxExport { }

public record SyntaxWhileStatement(ISyntaxNode Condition, ISyntaxNode Body, NodeLocation Location) : SyntaxNodeBase(Location) { }

public record SyntaxDoWhileStatement(ISyntaxNode Condition, ISyntaxNode Body, NodeLocation Location) : SyntaxNodeBase(Location) { }

public record SyntaxAssignmentStatement(NodeMode Mode, ISyntaxNode Target, ISyntaxNode Expression, NodeLocation Location) : SyntaxNodeBase(Location), ISyntaxMode { }

public record SyntaxBreakStatement(NodeLocation Location) : SyntaxNodeBase(Location);

public record SyntaxContinueStatement(NodeLocation Location) : SyntaxNodeBase(Location);

public record SyntaxReturnStatement(ISyntaxNode? Expression, bool IsForceReturnFunc, NodeLocation Location) : SyntaxNodeBase(Location), ISyntaxForceReturn { }

public record SyntaxIfExpression(ISyntaxNode Condition, ISyntaxNode ThenExpr, ISyntaxNode? ElseExpr, NodeLocation Location) : SyntaxNodeBase(Location) { }

public record SyntaxExpressionStatement(ISyntaxNode Expression, NodeLocation Location) : SyntaxNodeBase(Location) { }

public enum ImportMode
{
    All,
    Specific,
}

public record SyntaxImportDeclaration(ImportMode Mode, ISyntaxNode? Members, ISyntaxNode Source, NodeLocation Location) : SyntaxNodeBase(Location), ISyntaxImportMode { }

public record SyntaxNumberLiteral(long Value, NodeLocation Location) : SyntaxNodeBase(Location), ISyntaxNumberValue;

public record SyntaxUnaryOperation(NodeMode Mode, ISyntaxNode Expression, NodeLocation Location) : SyntaxNodeBase(Location), ISyntaxMode { }

public record SyntaxBinaryOperation(NodeMode Mode, ISyntaxNode Left, ISyntaxNode Right, NodeLocation Location) : SyntaxNodeBase(Location), ISyntaxMode { }

public record SyntaxIfStatement(ISyntaxNode Condition, ISyntaxNode ThenStatement, ISyntaxNode? ElseStatement, NodeLocation Location) : SyntaxNodeBase(Location) { }

public record SyntaxBlockExpression(List<ISyntaxNode> Body, NodeLocation Location) : SyntaxNodeBase(Location) { }

public record SyntaxReference(string Name, NodeLocation Location) : SyntaxNodeBase(Location), ISyntaxName;

public record SyntaxCall(ISyntaxNode Callee, List<ISyntaxNode> Args, NodeLocation Location) : SyntaxNodeBase(Location) { }

public record SyntaxIndexRef(ISyntaxNode Source, ISyntaxNode Index, NodeLocation Location) : SyntaxNodeBase(Location) { }

public record SyntaxCollectionExpression(List<ISyntaxNode> Elements, NodeLocation Location) : SyntaxNodeBase(Location) { }

public record SyntaxNamedType(string Name, NodeLocation Location) : SyntaxNodeBase(Location), ISyntaxName;

public record SyntaxCollectionType(ISyntaxNode ElementType, long? Size, NodeLocation Location) : SyntaxNodeBase(Location), ISyntaxCollectionSize { }

public record SyntaxPointerType(ISyntaxNode ElementType, NodeLocation Location) : SyntaxNodeBase(Location) { }

