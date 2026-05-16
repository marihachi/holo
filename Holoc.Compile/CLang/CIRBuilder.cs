using Holoc.Compile.Holo;

namespace Holoc.Compile.CLang;

public class CIRBuilder
{
    public CUnit CUnit;

    private CVersion TargetVersion = CVersion.C89;

    /// <summary>
    /// C99の機能が使用できるかどうか
    /// - ビット幅指定の数値型がある
    /// - _Bool型がある
    /// </summary>
    private bool C99Feature => TargetVersion >= CVersion.C99;

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

    private void AddInclude(string header)
    {
        if (!CUnit!.Includes.Contains(header))
        {
            CUnit!.Includes.Add(header);
        }
    }

    private CFunctionDecl BuildFunctionDecl(HoloFunctionDecl decl)
    {
        string returnType;

        // C言語の仕様でmain関数の戻り値はintでなければならない
        if (decl.Name == "main")
        {
            returnType = "int";
        }
        else
        {
            returnType = MapType(decl.ReturnType);
        }

        var parameters = new List<CParam>();
        foreach (var p in decl.Parameters)
        {
            parameters.Add(new CParam(MapType(p.Type), p.Name));
        }

        var body = decl.Body != null ? BuildBlock(decl.Body) : null;
        return new CFunctionDecl(returnType, decl.Name, parameters, body);
    }

    private CBlock BuildBlock(List<HoloStmt> block)
    {
        var stmts = new List<CStmt>();
        foreach (var stmt in block)
        {
            stmts.Add(BuildStatement(stmt));
        }
        return new CBlock(stmts);
    }

    private CStmt BuildStatement(HoloStmt stmt)
    {
        if (stmt is HoloVariableDeclStmt varDecl)
        {
            return new CVariableDeclStmt(
                MapType(varDecl.Type),
                varDecl.Name,
                varDecl.Initializer != null ? BuildExpression(varDecl.Initializer) : null
            );
        }
        
        if (stmt is HoloAssignStmt assign)
        {
            return new CAssignStmt(
                BuildExpression(assign.Target),
                AssignOp(assign.Op),
                BuildExpression(assign.Value)
            );
        }
        
        if (stmt is HoloIfStmt ifStmt)
        {
            return BuildIfStmt(ifStmt);
        }
        
        if (stmt is HoloWhileStmt whileStmt)
        {
            return new CWhileStmt(
                BuildExpression(whileStmt.Condition),
                BuildBlock(whileStmt.Body)
            );
        }
        
        if (stmt is HoloBreakStmt)
        {
            return new CBreakStmt();
        }
        
        if (stmt is HoloContinueStmt)
        {
            return new CContinueStmt();
        }
        
        if (stmt is HoloReturnStmt ret)
        {
            return new CReturnStmt(
                ret.Value != null ? BuildExpression(ret.Value) : null
            );
        }
        
        if (stmt is HoloExprStmt exprStmt)
        {
            return new CExprStmt(BuildExpression(exprStmt.Expression));
        }
        
        if (stmt is HoloBlockStmt blockStmt)
        {
            return new CBlockStmt(BuildBlock(blockStmt.Block));
        }

        throw new NotSupportedException($"Unsupported statement: {stmt.GetType().Name}");
    }

    private CIfStmt BuildIfStmt(HoloIfStmt stmt)
    {
        return new CIfStmt(
            BuildExpression(stmt.Condition),
            BuildBlock(stmt.Then),
            stmt.Else != null ? BuildElse(stmt.Else) : null
        );
    }

    private CStmt BuildElse(HoloStmt stmt)
    {
        if (stmt is HoloIfStmt elseIf)
        {
            return BuildIfStmt(elseIf);
        }

        if (stmt is HoloBlockStmt block)
        {
            return new CBlockStmt(BuildBlock(block.Block));
        }

        return new CBlockStmt(new CBlock([BuildStatement(stmt)]));
    }

    private CExpr BuildExpression(HoloExpr expr)
    {
        if (expr is HoloNumberLiteral numLit)
        {
            return new CNumberLiteral(numLit.Value);
        }

        if (expr is HoloBoolLiteral boolLit)
        {
            if (C99Feature)
            {
                return new CBoolLiteral(boolLit.Value);
            }
            // boolをサポートしていなければ、intとして扱う。
            else
            {
                return new CNumberLiteral(boolLit.Value ? 1 : 0);
            }
        }

        if (expr is HoloReference reference)
        {
            return new CIdentifier(reference.Name);
        }

        if (expr is HoloUnaryExpr unary)
        {
            return new CUnaryExpr(
                unary.Op == HoloUnaryOp.Neg ? "-" : "+",
                BuildExpression(unary.Operand)
            );
        }

        if (expr is HoloBinaryExpr binary)
        {
            return new CBinaryExpr(
                BuildExpression(binary.Left),
                BinaryOp(binary.Op),
                BuildExpression(binary.Right)
            );
        }

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
        {
            return BuildWhenExpression(when.Arms);
        }

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
            {
                result = value;
            }
            else
            {
                result = new CTernaryExpr(BuildExpression(arm.Condition), value, result);
            }
        }

        return result;
    }

    private static string AssignOp(HoloAssignOp op)
    {
        switch (op)
        {
            case HoloAssignOp.Add: return "+=";
            case HoloAssignOp.Sub: return "-=";
            case HoloAssignOp.Mul: return "*=";
            case HoloAssignOp.Div: return "/=";
            case HoloAssignOp.Rem: return "%=";
            case HoloAssignOp.BitAnd: return "&=";
            case HoloAssignOp.BitOr: return "|=";
            case HoloAssignOp.Xor: return "^=";
            case HoloAssignOp.ShiftLeft: return "<<=";
            case HoloAssignOp.ShiftRight: return ">>=";
            case HoloAssignOp.None: return "=";
        }
        throw new NotSupportedException($"Unsupported assign op: {op}");
    }

    private static string BinaryOp(HoloBinaryOp op)
    {
        switch (op)
        {
            case HoloBinaryOp.Add: return "+";
            case HoloBinaryOp.Sub: return "-";
            case HoloBinaryOp.Mul: return "*";
            case HoloBinaryOp.Div: return "/";
            case HoloBinaryOp.Rem: return "%";
            case HoloBinaryOp.ShiftLeft: return "<<";
            case HoloBinaryOp.ShiftRight: return ">>";
            case HoloBinaryOp.BitAnd: return "&";
            case HoloBinaryOp.BitOr: return "|";
            case HoloBinaryOp.Xor: return "^";
            case HoloBinaryOp.Gt: return ">";
            case HoloBinaryOp.Lt: return "<";
            case HoloBinaryOp.GtEq: return ">=";
            case HoloBinaryOp.LtEq: return "<=";
            case HoloBinaryOp.Eq: return "==";
            case HoloBinaryOp.NotEq: return "!=";
        }
        throw new NotSupportedException($"Unsupported binary op: {op}");
    }

    private string MapType(string holoType)
    {
        // holo言語のエイリアス
        switch (holoType)
        {
            case "int":
                holoType = "int32";
                break;

            case "uint":
                holoType = "uint32";
                break;

            case "byte":
                holoType = "uint8";
                break;
        }

        string cType;

        // C言語の型にマッピング
        switch (holoType)
        {
            case "int8":
                cType = C99Feature ? "int8_t" : "char";
                break;

            case "int16":
                cType = C99Feature ? "int16_t" : "short";
                break;

            case "int32":
                cType = C99Feature ? "int32_t" : "int";
                break;

            case "int64":
                cType = C99Feature ? "int64_t" : "long";
                break;

            case "uint8":
                cType = C99Feature ? "uint8_t" : "unsigned char";
                break;

            case "uint16":
                cType = C99Feature ? "uint16_t" : "unsigned short";
                break;

            case "uint32":
                cType = C99Feature ? "uint32_t" : "unsigned int";
                break;

            case "uint64":
                cType = C99Feature ? "uint64_t" : "unsigned long";
                break;

            case "float32":
                cType = "float";
                break;

            case "float64":
                cType = "double";
                break;

            case "void":
                cType = "void";
                break;

            case "bool":
                cType = C99Feature ? "bool" : "int";
                break;

            default:
                cType = holoType;
                break; // その他の型はそのまま使う（ユーザー定義型など）
        }

        // include追加

        if (C99Feature)
        {
            var stdIntTypes = new[] { "int8_t", "int16_t", "int32_t", "int64_t", "uint8_t", "uint16_t", "uint32_t", "uint64_t" };

            if (stdIntTypes.Contains(cType))
            {
                AddInclude("<stdint.h>");
            }

            if (cType == "bool")
            {
                AddInclude("<stdbool.h>");
            }
        }

        return cType;
    }
}
