using Holoc.Compile.Syntax.Node;

namespace Holoc.Compile.IR;

public class HoloIRBuilder
{
    public HoloUnit Build(SyntaxNode unit)
    {
        return new HoloUnit(unit.Body!.Select(BuildFunctionDecl).ToList());
    }

    private HoloFunctionDecl BuildFunctionDecl(SyntaxNode node)
    {
        var returnType = node.Operands![0]?.Name ?? "void";

        var parameters = (node.Parameters ?? [])
            .Select(p => new HoloParam(p.Name!, p.Operands?[0]?.Name ?? "int"))
            .ToList();

        var body = node.IsExternal ? null : BuildBlock(node.Body ?? []);

        return new HoloFunctionDecl(node.Name!, returnType, parameters, body);
    }

    private HoloBlock BuildBlock(List<SyntaxNode> stmts)
    {
        return new HoloBlock(stmts.Select(BuildStatement).ToList());
    }

    private HoloBlock BuildInlineBlock(SyntaxNode node)
    {
        if (node.Kind == NodeKind.BlockExpression)
            return BuildBlock(node.Body ?? []);

        return new HoloBlock([BuildStatement(node)]);
    }

    private HoloStmt BuildStatement(SyntaxNode node)
    {
        if (node.Kind == NodeKind.VariableDeclaration)
            return new HoloVariableDeclStmt(
                node.Name!,
                node.Operands![0]?.Name ?? "int",
                node.Operands[1] is { } init ? BuildExpression(init) : null
            );

        if (node.Kind == NodeKind.AssignStatement)
            return new HoloAssignStmt(
                BuildExpression(node.Operands![0]!),
                ToAssignOp(node.Mode),
                BuildExpression(node.Operands[1]!)
            );

        if (node.Kind == NodeKind.IfStatement)
            return BuildIfStmt(node);

        if (node.Kind == NodeKind.WhileStatement)
            return new HoloWhileStmt(
                BuildExpression(node.Operands![0]!),
                BuildInlineBlock(node.Operands[1]!)
            );

        if (node.Kind == NodeKind.BreakStatement)
            return new HoloBreakStmt();

        if (node.Kind == NodeKind.ContinueStatement)
            return new HoloContinueStmt();

        if (node.Kind == NodeKind.ReturnStatement)
            return new HoloReturnStmt(
                node.Operands?[0] is { } ret ? BuildExpression(ret) : null
            );

        if (node.Kind == NodeKind.ExpressionStatement)
            return new HoloExprStmt(BuildExpression(node.Operands![0]!));

        if (node.Kind == NodeKind.BlockExpression)
            return new HoloBlockStmt(BuildBlock(node.Body ?? []));

        throw new NotSupportedException($"Unsupported statement: {node.Kind}");
    }

    private HoloIfStmt BuildIfStmt(SyntaxNode node)
    {
        return new HoloIfStmt(
            BuildExpression(node.Operands![0]!),
            BuildInlineBlock(node.Operands[1]!),
            node.Operands[2] is { } elseNode ? BuildElseStmt(elseNode) : null
        );
    }

    private HoloStmt BuildElseStmt(SyntaxNode node)
    {
        if (node.Kind == NodeKind.IfStatement)
            return BuildIfStmt(node);

        return new HoloBlockStmt(BuildInlineBlock(node));
    }

    private HoloExpr BuildExpression(SyntaxNode node)
    {
        if (node.Kind == NodeKind.NumberLiteral)
            return new HoloNumberLiteral(node.Value!);

        if (node.Kind == NodeKind.Reference)
            return new HoloReference(node.Name!);

        if (node.Kind == NodeKind.UnaryOperation)
            return new HoloUnaryExpr(
                node.Mode == NodeMode.Sub ? HoloUnaryOp.Neg : HoloUnaryOp.Pos,
                BuildExpression(node.Operands![0]!)
            );

        if (node.Kind == NodeKind.BinaryOperation)
            return new HoloBinaryExpr(
                BuildExpression(node.Operands![0]!),
                ToBinaryOp(node.Mode),
                BuildExpression(node.Operands[1]!)
            );

        if (node.Kind == NodeKind.Call)
            return new HoloCallExpr(
                BuildExpression(node.Operands![0]!),
                (node.Body ?? []).Select(BuildExpression).ToList()
            );

        if (node.Kind == NodeKind.WhenExpression)
            return new HoloWhenExpr(
                node.Body!.Select(arm => new HoloWhenArm(
                    arm.Mode == NodeMode.DefaultArm ? null : BuildExpression(arm.Operands![1]!),
                    BuildExpression(arm.Operands![0]!)
                )).ToList()
            );

        if (node.Kind == NodeKind.BlockExpression)
            return new HoloBlockExpr(
                (node.Body ?? [])
                    .Where(n => n.Kind == NodeKind.ExpressionStatement)
                    .Select(n => BuildExpression(n.Operands![0]!))
                    .ToList()
            );

        throw new NotSupportedException($"Unsupported expression: {node.Kind}");
    }

    private static HoloAssignOp ToAssignOp(NodeMode mode)
    {
        if (mode == NodeMode.Add) return HoloAssignOp.Add;
        if (mode == NodeMode.Sub) return HoloAssignOp.Sub;
        if (mode == NodeMode.Mul) return HoloAssignOp.Mul;
        if (mode == NodeMode.Div) return HoloAssignOp.Div;
        if (mode == NodeMode.Rem) return HoloAssignOp.Rem;
        if (mode == NodeMode.BitAnd) return HoloAssignOp.BitAnd;
        if (mode == NodeMode.BitOr) return HoloAssignOp.BitOr;
        if (mode == NodeMode.Xor) return HoloAssignOp.Xor;
        if (mode == NodeMode.ShiftLeft) return HoloAssignOp.ShiftLeft;
        if (mode == NodeMode.ShiftRight) return HoloAssignOp.ShiftRight;
        return HoloAssignOp.Assign;
    }

    private static HoloBinaryOp ToBinaryOp(NodeMode mode)
    {
        if (mode == NodeMode.Add) return HoloBinaryOp.Add;
        if (mode == NodeMode.Sub) return HoloBinaryOp.Sub;
        if (mode == NodeMode.Mul) return HoloBinaryOp.Mul;
        if (mode == NodeMode.Div) return HoloBinaryOp.Div;
        if (mode == NodeMode.Rem) return HoloBinaryOp.Rem;
        if (mode == NodeMode.ShiftLeft) return HoloBinaryOp.ShiftLeft;
        if (mode == NodeMode.ShiftRight) return HoloBinaryOp.ShiftRight;
        if (mode == NodeMode.BitAnd) return HoloBinaryOp.BitAnd;
        if (mode == NodeMode.BitOr) return HoloBinaryOp.BitOr;
        if (mode == NodeMode.Xor) return HoloBinaryOp.Xor;
        if (mode == NodeMode.Gt) return HoloBinaryOp.Gt;
        if (mode == NodeMode.Lt) return HoloBinaryOp.Lt;
        if (mode == NodeMode.GtEq) return HoloBinaryOp.GtEq;
        if (mode == NodeMode.LtEq) return HoloBinaryOp.LtEq;
        if (mode == NodeMode.Eq) return HoloBinaryOp.Eq;
        if (mode == NodeMode.NotEq) return HoloBinaryOp.NotEq;
        throw new NotSupportedException($"Unsupported binary op: {mode}");
    }
}
