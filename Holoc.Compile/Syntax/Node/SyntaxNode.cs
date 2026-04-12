namespace Holoc.Compile.Syntax.Node;

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
    public bool IsExternal { get; set; }

    public static SyntaxNode CreateUnit(List<SyntaxNode> body, NodeLocation location)
    {
        return new SyntaxNode
        {
            Kind = NodeKind.Unit,
            Location = location,
            Body = body,
        };
    }

    public static SyntaxNode CreateFunctionDecl
        (string name, SyntaxNode? returnType, List<SyntaxNode>? parameters, List<SyntaxNode>? body, bool isExternal, NodeLocation location)
    {
        return new SyntaxNode
        {
            Kind = NodeKind.FunctionDecl,
            Location = location,
            Name = name,
            Operands = [returnType],
            Parameters = parameters,
            Body = body,
            IsExternal = isExternal,
        };
    }

    public static SyntaxNode CreateFunctionParameter
        (string name, SyntaxNode? paramType, NodeLocation location)
    {
        return new SyntaxNode
        {
            Kind = NodeKind.FunctionParameter,
            Location = location,
            Operands = [paramType],
            Name = name,
        };
    }

    public static SyntaxNode CreateVariableDecl
        (string name, SyntaxNode? variableType, SyntaxNode? initializer, NodeLocation location)
    {
        return new SyntaxNode
        {
            Kind = NodeKind.VariableDecl,
            Location = location,
            Name = name,
            Operands = [variableType, initializer],
        };
    }

    public static SyntaxNode CreateWhileStatement
        (SyntaxNode condition, SyntaxNode body, NodeLocation location)
    {
        return new SyntaxNode
        {
            Kind = NodeKind.WhileStatement,
            Location = location,
            Operands = [condition, body],
        };
    }

    public static SyntaxNode CreateAssignStatement
        (NodeMode assignMode, SyntaxNode assignee, SyntaxNode expression, NodeLocation location)
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
            Operands = [expression],
        };
    }

    public static SyntaxNode CreateSwitchStatement
        (SyntaxNode condition, List<SyntaxNode> arms, NodeLocation location)
    {
        return new SyntaxNode
        {
            Kind = NodeKind.SwitchStatement,
            Location = location,
            Operands = [condition],
            Body = arms,
        };
    }

    public static SyntaxNode CreateSwitchArm
        (bool isDefaultArm, SyntaxNode? condition, SyntaxNode expression, NodeLocation location)
    {
        return new SyntaxNode
        {
            Kind = NodeKind.SwitchArm,
            Location = location,
            Mode = isDefaultArm ? NodeMode.DefaultArm : NodeMode.None,
            Operands = [expression, condition],
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

    public static SyntaxNode CreateBinaryOperation
        (SyntaxNode left, SyntaxNode right, NodeLocation location)
    {
        return new SyntaxNode
        {
            Kind = NodeKind.BinaryOperation,
            Location = location,
            Operands = [left, right],
        };
    }

    public static SyntaxNode CreateIf
        (SyntaxNode condition, SyntaxNode thenExpression, SyntaxNode? elseExpression, NodeLocation location)
    {
        return new SyntaxNode
        {
            Kind = NodeKind.If,
            Location = location,
            Operands = [condition, thenExpression, elseExpression],
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

    public static SyntaxNode CreateCall
        (SyntaxNode callee, List<SyntaxNode> args, NodeLocation location)
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

        if (node.IsExternal)
        {
            Console.Write(" [External]");
        }

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
                    ShowSyntaxNodeInternal(operand, nextIndent, i == node.Operands.Count - 1 && (node.Parameters == null || node.Parameters.Count == 0) && (node.Body == null || node.Body.Count == 0), "O");
                else
                    Console.WriteLine(nextIndent + (i == node.Operands.Count - 1 && (node.Parameters == null || node.Parameters.Count == 0) && (node.Body == null || node.Body.Count == 0) ? "└── " : "├── ") + "[O] <null>");
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
