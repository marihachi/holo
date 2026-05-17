using Holoc.Compile.Holo.Syntax.Node;

namespace Holoc.Compile.Holo;

/// <summary>
/// Holo IRの構築、識別子の解決、型チェックを行います。
/// </summary>
public class HoloIRBuilder
{
    public HoloUnit HoloUnit;

    public HoloIRBuilder()
    {
        HoloUnit = new HoloUnit("", new List<IHoloDecl>());
    }

    public void Clear()
    {
        HoloUnit = new HoloUnit("", new List<IHoloDecl>());
    }

    public void Build(string holoFileName, SyntaxNode unit)
    {
        if (unit.Kind != NodeKind.Unit)
        {
            throw new NotSupportedException($"Unsupported node kind: {unit.Kind}");
        }

        var decls = new List<IHoloDecl>();
        foreach (var node in unit.Body!)
        {
            if (node.Kind == NodeKind.FunctionDeclaration)
            {
                decls.Add(BuildFunctionDecl(node));
            }
        }
        HoloUnit = new HoloUnit(holoFileName, decls);
    }

    private HoloFunctionDecl BuildFunctionDecl(SyntaxNode node)
    {
        var returnType = node.Operands![0]?.Name ?? "void";

        var parameters = new List<HoloParam>();
        foreach (var p in node.Parameters ?? [])
        {
            parameters.Add(new HoloParam(p.Name!, p.Operands?[0]?.Name ?? "int"));
        }

        var body = node.IsExternal ? null : BuildBlock(node.Body ?? []);

        return new HoloFunctionDecl(node.Name!, returnType, parameters, body);
    }

    private List<IHoloStmt> BuildBlock(List<SyntaxNode> stmts)
    {
        var statements = new List<IHoloStmt>();
        foreach (var stmt in stmts)
        {
            statements.Add(BuildStatement(stmt));
        }
        return statements;
    }

    private List<IHoloStmt> BuildInlineBlock(SyntaxNode node)
    {
        if (node.Kind == NodeKind.BlockExpression)
        {
            return BuildBlock(node.Body ?? []);
        }

        return [BuildStatement(node)];
    }

    private IHoloStmt BuildStatement(SyntaxNode node)
    {
        if (node.Kind == NodeKind.VariableDeclaration)
        {
            return new HoloVariableDeclStmt(
                node.Name!,
                node.Operands![0]?.Name ?? "int",
                node.Operands[1] is { } init ? BuildExpression(init) : null
            );
        }

        if (node.Kind == NodeKind.AssignStatement)
        {
            return new HoloAssignStmt(
                BuildExpression(node.Operands![0]!),
                ToAssignOp(node.Mode),
                BuildExpression(node.Operands[1]!)
            );
        }

        if (node.Kind == NodeKind.IfStatement)
        {
            return BuildIfStmt(node);
        }

        if (node.Kind == NodeKind.WhileStatement)
        {
            return new HoloWhileStmt(
                BuildExpression(node.Operands![0]!),
                BuildInlineBlock(node.Operands[1]!)
            );
        }

        if (node.Kind == NodeKind.BreakStatement)
        {
            return new HoloBreakStmt();
        }

        if (node.Kind == NodeKind.ContinueStatement)
        {
            return new HoloContinueStmt();
        }

        if (node.Kind == NodeKind.ReturnStatement)
        {
            return new HoloReturnStmt(
                node.Operands?[0] is { } ret ? BuildExpression(ret) : null
            );
        }

        if (node.Kind == NodeKind.ExpressionStatement)
        {
            return new HoloExprStmt(BuildExpression(node.Operands![0]!));
        }

        if (node.Kind == NodeKind.BlockExpression)
        {
            return new HoloBlockStmt(BuildBlock(node.Body ?? []));
        }

        throw new NotSupportedException($"Unsupported statement: {node.Kind}");
    }

    private HoloIfStmt BuildIfStmt(SyntaxNode node)
    {
        return new HoloIfStmt(
            BuildExpression(node.Operands![0]!),
            BuildInlineBlock(node.Operands[1]!),
            node.Operands[2] is { } elseNode ? BuildElse(elseNode) : null
        );
    }

    private IHoloStmt BuildElse(SyntaxNode node)
    {
        if (node.Kind == NodeKind.IfStatement)
        {
            return BuildIfStmt(node);
        }

        return new HoloBlockStmt(BuildInlineBlock(node));
    }

    private IHoloExpr BuildExpression(SyntaxNode node)
    {
        if (node.Kind == NodeKind.NumberLiteral)
        {
            return new HoloNumberLiteral((long)node.Value!);
        }

        if (node.Kind == NodeKind.Reference)
        {
            var name = node.Name!;

            if (name == "true" || name == "false")
            {
                return new HoloBoolLiteral(name == "true");
            }

            return new HoloReference(node.Name!);
        }

        if (node.Kind == NodeKind.UnaryOperation)
        {
            return new HoloUnaryExpr(
                node.Mode == NodeMode.Sub ? HoloUnaryOp.Neg : HoloUnaryOp.Pos,
                BuildExpression(node.Operands![0]!)
            );
        }

        if (node.Kind == NodeKind.BinaryOperation)
        {
            return new HoloBinaryExpr(
                BuildExpression(node.Operands![0]!),
                ToBinaryOp(node.Mode),
                BuildExpression(node.Operands[1]!)
            );
        }

        if (node.Kind == NodeKind.Call)
        {
            var args = new List<IHoloExpr>();
            foreach (var arg in node.Body ?? [])
            {
                args.Add(BuildExpression(arg));
            }
            return new HoloCallExpr(BuildExpression(node.Operands![0]!), args);
        }

        if (node.Kind == NodeKind.WhenExpression)
        {
            var whenArms = new List<HoloWhenArm>();
            foreach (var arm in node.Body!)
            {
                var condition = arm.Mode == NodeMode.DefaultArm ? null : BuildExpression(arm.Operands![1]!);
                var value = BuildExpression(arm.Operands![0]!);
                whenArms.Add(new HoloWhenArm(condition, value));
            }
            return new HoloWhenExpr(whenArms);
        }

        if (node.Kind == NodeKind.BlockExpression)
        {
            var blockExprs = new List<IHoloExpr>();
            foreach (var n in node.Body ?? [])
            {
                if (n.Kind == NodeKind.ExpressionStatement)
                {
                    blockExprs.Add(BuildExpression(n.Operands![0]!));
                }
            }
            return new HoloBlockExpr(blockExprs);
        }

        throw new NotSupportedException($"Unsupported expression: {node.Kind}");
    }

    private static HoloAssignOp ToAssignOp(NodeMode mode)
    {
        switch (mode)
        {
            case NodeMode.Add: return HoloAssignOp.Add;
            case NodeMode.Sub: return HoloAssignOp.Sub;
            case NodeMode.Mul: return HoloAssignOp.Mul;
            case NodeMode.Div: return HoloAssignOp.Div;
            case NodeMode.Rem: return HoloAssignOp.Rem;
            case NodeMode.BitAnd: return HoloAssignOp.BitAnd;
            case NodeMode.BitOr: return HoloAssignOp.BitOr;
            case NodeMode.Xor: return HoloAssignOp.Xor;
            case NodeMode.ShiftLeft: return HoloAssignOp.ShiftLeft;
            case NodeMode.ShiftRight: return HoloAssignOp.ShiftRight;
            default: return HoloAssignOp.None;
        }
    }

    private static HoloBinaryOp ToBinaryOp(NodeMode mode)
    {
        switch (mode)
        {
            case NodeMode.Add: return HoloBinaryOp.Add;
            case NodeMode.Sub: return HoloBinaryOp.Sub;
            case NodeMode.Mul: return HoloBinaryOp.Mul;
            case NodeMode.Div: return HoloBinaryOp.Div;
            case NodeMode.Rem: return HoloBinaryOp.Rem;
            case NodeMode.ShiftLeft: return HoloBinaryOp.ShiftLeft;
            case NodeMode.ShiftRight: return HoloBinaryOp.ShiftRight;
            case NodeMode.BitAnd: return HoloBinaryOp.BitAnd;
            case NodeMode.BitOr: return HoloBinaryOp.BitOr;
            case NodeMode.Xor: return HoloBinaryOp.Xor;
            case NodeMode.Gt: return HoloBinaryOp.Gt;
            case NodeMode.Lt: return HoloBinaryOp.Lt;
            case NodeMode.GtEq: return HoloBinaryOp.GtEq;
            case NodeMode.LtEq: return HoloBinaryOp.LtEq;
            case NodeMode.Eq: return HoloBinaryOp.Eq;
            case NodeMode.NotEq: return HoloBinaryOp.NotEq;
            default:
                throw new NotSupportedException($"Unsupported binary op: {mode}");
        }
    }
}
