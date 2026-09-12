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

    public void Build(string holoFileName, ISyntaxNode unit)
    {
        if (unit is not SyntaxUnit unitNode)
        {
            throw new NotSupportedException($"Unsupported node: {unit.GetType().Name}");
        }

        var decls = new List<IHoloDecl>();
        foreach (var node in unitNode.Body)
        {
            if (node is SyntaxFunctionDecl funcDecl)
            {
                decls.Add(BuildFunctionDecl(funcDecl));
            }

            if (node is SyntaxVariableDecl varDecl)
            {
                decls.Add(BuildVariableDecl(varDecl, true));
            }
        }
        HoloUnit = new HoloUnit(holoFileName, decls);
    }

    private HoloFunctionDecl BuildFunctionDecl(SyntaxFunctionDecl node)
    {
        var modifiers = HoloDeclModifier.None;

        if (node.IsDeclare)
        {
            modifiers |= HoloDeclModifier.Declare;
        }

        if (node.IsExport)
        {
            modifiers |= HoloDeclModifier.Export;
        }

        var returnType = node.ReturnType;

        if (returnType == null)
        {
            throw new NotSupportedException($"The return type is not specified");
        }

        var parameters = new List<HoloParam>();
        foreach (var p in node.Parameters ?? [])
        {
            if (p is not SyntaxFunctionParameter param)
            {
                throw new NotSupportedException($"Unsupported parameter node: {p.GetType().Name}");
            }

            if (param.ParamType == null)
            {
                throw new NotSupportedException($"A parameter type is not specified");
            }

            parameters.Add(new HoloParam(param.Name, BuildType(param.ParamType)));
        }

        var body = node.IsDeclare ? null : BuildStatements(node.Body ?? []);

        return new HoloFunctionDecl(node.Name, BuildType(returnType), parameters, body, modifiers);
    }

    private IHoloType BuildType(ISyntaxNode node)
    {
        if (node is SyntaxNamedType namedType)
        {
            return new HoloNamedType(namedType.Name);
        }

        if (node is SyntaxCollectionType collectionType)
        {
            return new HoloCollectionType(BuildType(collectionType.ElementType), collectionType.Size);
        }

        if (node is SyntaxPointerType pointerType)
        {
            return new HoloPointerType(BuildType(pointerType.ElementType));
        }

        throw new NotSupportedException($"Unsupported type node: {node.GetType().Name}");
    }

    private List<IHoloStmt> BuildStatements(List<ISyntaxNode> stmts)
    {
        var statements = new List<IHoloStmt>();
        foreach (var stmt in stmts)
        {
            statements.Add(BuildStatement(stmt));
        }
        return statements;
    }

    private List<IHoloStmt> BuildInlineBlock(ISyntaxNode node)
    {
        if (node is SyntaxBlockExpression blockExpr)
        {
            return BuildStatements(blockExpr.Body);
        }

        return [BuildStatement(node)];
    }

    private IHoloStmt BuildStatement(ISyntaxNode node)
    {
        if (node is SyntaxVariableDecl varDecl)
        {
            return BuildVariableDecl(varDecl, false);
        }

        if (node is SyntaxAssignmentStatement assign)
        {
            return new HoloAssignStmt(
                BuildExpression(assign.Target),
                ToAssignOp(assign.Mode),
                BuildExpression(assign.Expression)
            );
        }

        if (node is SyntaxIfStatement ifStmt)
        {
            return BuildIfStmt(ifStmt);
        }

        if (node is SyntaxWhileStatement whileStmt)
        {
            return new HoloWhileStmt(
                BuildExpression(whileStmt.Condition),
                BuildInlineBlock(whileStmt.Body)
            );
        }

        if (node is SyntaxDoWhileStatement doWhileStmt)
        {
            return new HoloDoWhileStmt(
                BuildExpression(doWhileStmt.Condition),
                BuildInlineBlock(doWhileStmt.Body)
            );
        }

        if (node is SyntaxBreakStatement)
        {
            return new HoloBreakStmt();
        }

        if (node is SyntaxContinueStatement)
        {
            return new HoloContinueStmt();
        }

        if (node is SyntaxReturnStatement returnStmt)
        {
            return new HoloReturnStmt(
                returnStmt.Expression != null ? BuildExpression(returnStmt.Expression) : null
            );
        }

        if (node is SyntaxExpressionStatement exprStmt)
        {
            return new HoloExprStmt(BuildExpression(exprStmt.Expression));
        }

        if (node is SyntaxBlockExpression blockExpr)
        {
            return new HoloBlockStmt(BuildStatements(blockExpr.Body));
        }

        throw new NotSupportedException($"Unsupported statement: {node.GetType().Name}");
    }

    private HoloVariableDeclStmt BuildVariableDecl(SyntaxVariableDecl node, bool isTopLevel)
    {
        var modifiers = HoloDeclModifier.None;

        if (isTopLevel)
        {
            if (node.IsDeclare)
            {
                modifiers |= HoloDeclModifier.Declare;
            }

            if (node.IsExport)
            {
                modifiers |= HoloDeclModifier.Export;
            }
        }

        var variableType = node.VariableType;

        if (variableType == null)
        {
            throw new NotSupportedException($"The variable type is not specified");
        }

        return new HoloVariableDeclStmt(
            node.Name,
            BuildType(variableType),
            node.Initializer != null ? BuildExpression(node.Initializer) : null,
            modifiers
        );
    }

    private HoloIfStmt BuildIfStmt(SyntaxIfStatement node)
    {
        return new HoloIfStmt(
            BuildExpression(node.Condition),
            BuildInlineBlock(node.ThenStatement),
            node.ElseStatement != null ? BuildElse(node.ElseStatement) : null
        );
    }

    private IHoloStmt BuildElse(ISyntaxNode node)
    {
        if (node is SyntaxIfStatement ifStmt)
        {
            return BuildIfStmt(ifStmt);
        }

        return new HoloBlockStmt(BuildInlineBlock(node));
    }

    private IHoloExpr BuildExpression(ISyntaxNode node)
    {
        if (node is SyntaxNumberLiteral numberLiteral)
        {
            return new HoloNumberLiteral(numberLiteral.Value);
        }

        if (node is SyntaxReference reference)
        {
            var name = reference.Name;

            if (name == "true" || name == "false")
            {
                return new HoloBoolLiteral(name == "true");
            }

            return new HoloIdentifier(name);
        }

        if (node is SyntaxUnaryOperation unaryOperation)
        {
            return new HoloUnaryExpr(
                unaryOperation.Mode == NodeMode.Sub ? HoloUnaryOp.Neg : HoloUnaryOp.Pos,
                BuildExpression(unaryOperation.Expression)
            );
        }

        if (node is SyntaxBinaryOperation binaryOperation)
        {
            return new HoloBinaryExpr(
                BuildExpression(binaryOperation.Left),
                ToBinaryOp(binaryOperation.Mode),
                BuildExpression(binaryOperation.Right)
            );
        }

        if (node is SyntaxGroupExpression groupExpr)
        {
            return new HoloGroupExpr(BuildExpression(groupExpr.Expression));
        }

        if (node is SyntaxCall call)
        {
            var args = new List<IHoloExpr>();
            foreach (var arg in call.Args)
            {
                args.Add(BuildExpression(arg));
            }
            return new HoloCallExpr(BuildExpression(call.Callee), args);
        }

        if (node is SyntaxIndexRef indexRef)
        {
            return new HoloIndexRefExpr(
                BuildExpression(indexRef.Source),
                BuildExpression(indexRef.Index)
            );
        }

        if (node is SyntaxIfExpression ifExpr)
        {
            // if式ではelse句は必須。
            if (ifExpr.ElseExpr == null)
            {
                throw new NotSupportedException($"The if expression needs an else clause.");
            }

            return new HoloIfExpr(BuildExpression(ifExpr.Condition), BuildExpression(ifExpr.ThenExpr), BuildExpression(ifExpr.ElseExpr));
        }

        if (node is SyntaxBlockExpression blockExpr)
        {
            var blockExprs = new List<IHoloExpr>();
            foreach (var n in blockExpr.Body)
            {
                if (n is SyntaxExpressionStatement exprStmt)
                {
                    blockExprs.Add(BuildExpression(exprStmt.Expression));
                }
            }
            return new HoloBlockExpr(blockExprs);
        }

        if (node is SyntaxCollectionExpression collectionExpr)
        {
            var elements = new List<IHoloExpr>();
            foreach (var n in collectionExpr.Elements)
            {
                elements.Add(BuildExpression(n));
            }
            return new HoloCollectionExpr(elements);
        }

        throw new NotSupportedException($"Unsupported expression: {node.GetType().Name}");
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
