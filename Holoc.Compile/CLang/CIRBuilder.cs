using Holoc.Compile.IR;

namespace Holoc.Compile.CLang;

public class CIRBuilder
{
    public CUnit CUnit;

    public CIRBuilder()
    {
        CUnit = new CUnit([], []);
    }

    public void Clear()
    {
        CUnit = new CUnit([], []);
    }

    public void Build(HoloUnit unit)
    {
        foreach (var decl in unit.Functions)
        {
            var cDecl = BuildFunctionDecl(decl);
            CUnit.Declarations.Add(cDecl);
        }
    }

    private CFunctionDecl BuildFunctionDecl(HoloFunctionDecl decl)
    {
        var returnType = MapType(decl.ReturnType);

        // C言語の仕様でmain関数の戻り値はintでなければならない
        if (decl.Name == "main")
            returnType = "int";

        var parameters = new List<CParam>();
        foreach (var p in decl.Parameters)
        {
            parameters.Add(new CParam(MapType(p.Type), p.Name));
        }
        var body = decl.Body != null ? BuildBlock(decl.Body) : null;
        return new CFunctionDecl(returnType, decl.Name, parameters, body);
    }

    private CBlock BuildBlock(HoloBlock block)
    {
        var stmts = new List<CStmt>();
        foreach (var stmt in block.Statements)
        {
            stmts.Add(BuildStatement(stmt));
        }
        return new CBlock(stmts);
    }

    private CStmt BuildStatement(HoloStmt stmt)
    {
        if (stmt is HoloVariableDeclStmt varDecl)
            return new CVariableDeclStmt(
                MapType(varDecl.Type),
                varDecl.Name,
                varDecl.Initializer != null ? BuildExpression(varDecl.Initializer) : null
            );

        if (stmt is HoloAssignStmt assign)
            return new CAssignStmt(
                BuildExpression(assign.Target),
                AssignOp(assign.Op),
                BuildExpression(assign.Value)
            );

        if (stmt is HoloIfStmt ifStmt)
            return BuildIfStmt(ifStmt);

        if (stmt is HoloWhileStmt whileStmt)
            return new CWhileStmt(
                BuildExpression(whileStmt.Condition),
                BuildBlock(whileStmt.Body)
            );

        if (stmt is HoloBreakStmt)
            return new CBreakStmt();

        if (stmt is HoloContinueStmt)
            return new CContinueStmt();

        if (stmt is HoloReturnStmt ret)
            return new CReturnStmt(
                ret.Value != null ? BuildExpression(ret.Value) : null
            );

        if (stmt is HoloExprStmt exprStmt)
            return new CExprStmt(BuildExpression(exprStmt.Expression));

        if (stmt is HoloBlockStmt blockStmt)
            return new CBlockStmt(BuildBlock(blockStmt.Block));

        throw new NotSupportedException($"Unsupported statement: {stmt.GetType().Name}");
    }

    private CIfStmt BuildIfStmt(HoloIfStmt stmt)
    {
        return new CIfStmt(
            BuildExpression(stmt.Condition),
            BuildBlock(stmt.Then),
            stmt.Else != null ? BuildElseStmt(stmt.Else) : null
        );
    }

    private CStmt BuildElseStmt(HoloStmt stmt)
    {
        if (stmt is HoloIfStmt elseIf)
            return BuildIfStmt(elseIf);

        if (stmt is HoloBlockStmt block)
            return new CBlockStmt(BuildBlock(block.Block));

        return new CBlockStmt(new CBlock([BuildStatement(stmt)]));
    }

    private CExpr BuildExpression(HoloExpr expr)
    {
        if (expr is HoloNumberLiteral numLit)
            return new CNumberLiteral(numLit.Value);

        if (expr is HoloReference reference)
            return new CIdentifier(reference.Name);

        if (expr is HoloUnaryExpr unary)
            return new CUnaryExpr(
                unary.Op == HoloUnaryOp.Neg ? "-" : "+",
                BuildExpression(unary.Operand)
            );

        if (expr is HoloBinaryExpr binary)
            return new CBinaryExpr(
                BuildExpression(binary.Left),
                BinaryOp(binary.Op),
                BuildExpression(binary.Right)
            );

        if (expr is HoloCallExpr call)
        {
            var args = new List<CExpr>();
            foreach (var arg in call.Args)
            {
                args.Add(BuildExpression(arg));
            }
            return new CCallExpr(BuildExpression(call.Callee), args);
        }

        if (expr is HoloWhenExpr when)
            return BuildWhenExpression(when.Arms);

        if (expr is HoloBlockExpr blockExpr)
        {
            var exprs = new List<CExpr>();
            foreach (var e in blockExpr.Expressions)
            {
                exprs.Add(BuildExpression(e));
            }
            return new CStmtExpr(exprs);
        }

        throw new NotSupportedException($"Unsupported expression: {expr.GetType().Name}");
    }

    private CExpr BuildWhenExpression(List<HoloWhenArm> arms)
    {
        // when式をネストした条件演算子に変換する。
        // 右から左に処理することで、条件付きアームが外側、elseアームが最も内側になる。
        // 例: when (cond1) v1 when (cond2) v2 else v3
        //   → (cond1 ? v1 : (cond2 ? v2 : (v3)))
        CExpr result = new CNumberLiteral(0); // elseアームがあれば到達しない

        for (int i = arms.Count - 1; i >= 0; i--)
        {
            var arm = arms[i];
            var value = BuildExpression(arm.Value);

            // Condition == null はelseアーム
            if (arm.Condition == null)
                result = value;
            else
                result = new CTernaryExpr(BuildExpression(arm.Condition), value, result);
        }

        return result;
    }

    private static string AssignOp(HoloAssignOp op)
    {
        if (op == HoloAssignOp.Add) return "+=";
        if (op == HoloAssignOp.Sub) return "-=";
        if (op == HoloAssignOp.Mul) return "*=";
        if (op == HoloAssignOp.Div) return "/=";
        if (op == HoloAssignOp.Rem) return "%=";
        if (op == HoloAssignOp.BitAnd) return "&=";
        if (op == HoloAssignOp.BitOr) return "|=";
        if (op == HoloAssignOp.Xor) return "^=";
        if (op == HoloAssignOp.ShiftLeft) return "<<=";
        if (op == HoloAssignOp.ShiftRight) return ">>=";
        return "=";
    }

    private static string BinaryOp(HoloBinaryOp op)
    {
        if (op == HoloBinaryOp.Add) return "+";
        if (op == HoloBinaryOp.Sub) return "-";
        if (op == HoloBinaryOp.Mul) return "*";
        if (op == HoloBinaryOp.Div) return "/";
        if (op == HoloBinaryOp.Rem) return "%";
        if (op == HoloBinaryOp.ShiftLeft) return "<<";
        if (op == HoloBinaryOp.ShiftRight) return ">>";
        if (op == HoloBinaryOp.BitAnd) return "&";
        if (op == HoloBinaryOp.BitOr) return "|";
        if (op == HoloBinaryOp.Xor) return "^";
        if (op == HoloBinaryOp.Gt) return ">";
        if (op == HoloBinaryOp.Lt) return "<";
        if (op == HoloBinaryOp.GtEq) return ">=";
        if (op == HoloBinaryOp.LtEq) return "<=";
        if (op == HoloBinaryOp.Eq) return "==";
        if (op == HoloBinaryOp.NotEq) return "!=";
        throw new NotSupportedException($"Unsupported binary op: {op}");
    }

    private string MapType(string holoType)
    {
        string cType;
        if (holoType == "int") cType = "int32_t";
        else if (holoType == "int8") cType = "int8_t";
        else if (holoType == "int16") cType = "int16_t";
        else if (holoType == "int32") cType = "int32_t";
        else if (holoType == "int64") cType = "int64_t";
        else if (holoType == "uint") cType = "uint32_t";
        else if (holoType == "uint8") cType = "uint8_t";
        else if (holoType == "uint16") cType = "uint16_t";
        else if (holoType == "uint32") cType = "uint32_t";
        else if (holoType == "uint64") cType = "uint64_t";
        else if (holoType == "float32") cType = "float";
        else if (holoType == "float64") cType = "double";
        else if (holoType == "bool") cType = "int";
        else if (holoType == "byte") cType = "uint8_t";
        else if (holoType == "void") cType = "void";
        else cType = holoType;

        if (cType.EndsWith("_t"))
            CUnit!.Includes.Add("<stdint.h>");

        return cType;
    }
}
