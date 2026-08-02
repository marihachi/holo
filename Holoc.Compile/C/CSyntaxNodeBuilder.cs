using Holoc.Compile.Holo;

namespace Holoc.Compile.C;

public class CSyntaxNodeBuilder
{
    public CFile CImpl;
    public CFile CHeader;

    private string holoFileName;

    private CVersion TargetVersion = CVersion.C11;

    /// <summary>
    /// C99の機能が使用できるかどうか
    /// - ビット幅指定の数値型がある
    /// - _Bool型がある
    /// </summary>
    private bool C99Feature => TargetVersion >= CVersion.C99;

    private enum IncludeAdd
    {
        Impl,
        Header,
    }

    public CSyntaxNodeBuilder()
    {
        holoFileName = "a.holo";
        CImpl = new CFile([], []);
        CHeader = new CFile([], []);
    }

    public void Clear()
    {
        holoFileName = "a.holo";
        CImpl = new CFile([], []);
        CHeader = new CFile([], []);
    }

    public void Build(HoloUnit unit)
    {
        holoFileName = unit.fileName;

        foreach (var decl in unit.Declarations)
        {
            if (decl is HoloFunctionDecl func)
            {
                var cDecl = BuildFunctionDecl(func);

                // exportが付いていればヘッダーで公開
                if (func.Modifiers.HasFlag(HoloDeclModifier.Export))
                {
                    CHeader.Declarations.Add(new CFunctionDecl(
                        cDecl.ReturnType,
                        cDecl.Name,
                        cDecl.Parameters,
                        null,
                        CDeclModifier.None
                    ));
                }

                CImpl.Declarations.Add(cDecl);
            }

            if (decl is HoloVariableDeclStmt varDecl)
            {
                // exportでなければstaticを付ける
                var cDecl = new CVariableDeclStmt(
                    BuildType(varDecl.Type, IncludeAdd.Header),
                    varDecl.Name,
                    varDecl.Initializer != null ? BuildExpression(varDecl.Initializer) : null,
                    !varDecl.Modifiers.HasFlag(HoloDeclModifier.Export) ? CDeclModifier.Static : CDeclModifier.None
                );

                // exportが付いていればヘッダーで公開
                if (varDecl.Modifiers.HasFlag(HoloDeclModifier.Export))
                {
                    CHeader.Declarations.Add(new CVariableDeclStmt(
                        cDecl.Type,
                        cDecl.Name,
                        null,
                        CDeclModifier.Extern
                    ));
                }

                CImpl.Declarations.Add(cDecl);
            }
        }
    }

    private void AddInclude(string header, IncludeAdd includeAdd)
    {
        // ヘッダーファイルにincludeを追加。
        if (includeAdd.HasFlag(IncludeAdd.Header) && !CHeader!.Includes.Contains(header))
        {
            CHeader!.Includes.Add(header);

            // 実装ファイルに同じincludeがあれば削除。
            if (CImpl!.Includes.Contains(header))
            {
                CImpl!.Includes.Remove(header);
            }
        }

        // 実装ファイルにincludeを追加。
        // ・既にヘッダー側にあれば追加しない。
        if (includeAdd.HasFlag(IncludeAdd.Impl) && !CHeader!.Includes.Contains(header) && !CImpl!.Includes.Contains(header))
        {
            CImpl!.Includes.Add(header);
        }
    }

    private ICType BuildType(IHoloType holoType, IncludeAdd includeAdd)
    {
        if (holoType is HoloNamedType namedType)
        {
            return new CNamedType(MapType(namedType, includeAdd));
        }

        if (holoType is HoloCollectionType collectionType)
        {
            return new CArrayType(BuildType(collectionType.elementType, includeAdd), collectionType.size);
        }

        if (holoType is HoloPointerType pointerType)
        {
            return new CPointerType(BuildType(pointerType.elementType, includeAdd));
        }

        throw new NotSupportedException($"Unsupported statement: {holoType.GetType().Name}");
    }

    private CFunctionDecl BuildFunctionDecl(HoloFunctionDecl decl)
    {
        ICType returnType;

        // C言語の仕様でmain関数の戻り値はintでなければならない
        if (decl.Name == "main")
        {
            returnType = new CNamedType("int");
        }
        else
        {
            returnType = BuildType(decl.ReturnType, IncludeAdd.Header);
        }

        var parameters = new List<CParam>();
        foreach (var p in decl.Parameters)
        {
            parameters.Add(new CParam(BuildType(p.Type, IncludeAdd.Header), p.Name));
        }

        if (parameters.Count == 0)
        {
            // C言語の仕様で引数なしはvoidと書く必要がある
            parameters.Add(new CParam(new CNamedType("void"), null));
        }

        var body = decl.Body != null ? BuildBlock(decl.Body) : null;

        // exportが付いていなければstaticを付ける
        var impl = new CFunctionDecl(
            returnType,
            decl.Name,
            parameters,
            body,
            !decl.Modifiers.HasFlag(HoloDeclModifier.Export) ? CDeclModifier.Static : CDeclModifier.None);

        AddInclude($"\"{Path.ChangeExtension(holoFileName, ".h")}\"", IncludeAdd.Impl);

        return impl;
    }

    private CBlock BuildBlock(List<IHoloStmt> block)
    {
        var stmts = new List<ICStmt>();
        foreach (var stmt in block)
        {
            stmts.Add(BuildStatement(stmt, IncludeAdd.Impl));
        }
        return new CBlock(stmts);
    }

    private ICStmt BuildStatement(IHoloStmt stmt, IncludeAdd includeAdd)
    {
        if (stmt is HoloVariableDeclStmt varDecl)
        {
            var variableType = BuildType(varDecl.Type, includeAdd);

            return new CVariableDeclStmt(
                variableType,
                varDecl.Name,
                varDecl.Initializer != null ? BuildExpression(varDecl.Initializer) : null,
                CDeclModifier.None
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
            return BuildIfStmt(ifStmt, includeAdd);
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

    private CIfStmt BuildIfStmt(HoloIfStmt stmt, IncludeAdd includeAdd)
    {
        return new CIfStmt(
            BuildExpression(stmt.Condition),
            BuildBlock(stmt.Then),
            stmt.Else != null ? BuildElse(stmt.Else, includeAdd) : null
        );
    }

    private ICStmt BuildElse(IHoloStmt stmt, IncludeAdd includeAdd)
    {
        if (stmt is HoloIfStmt elseIf)
        {
            return BuildIfStmt(elseIf, includeAdd);
        }

        if (stmt is HoloBlockStmt block)
        {
            return new CBlockStmt(BuildBlock(block.Block));
        }

        return new CBlockStmt(new CBlock([BuildStatement(stmt, includeAdd)]));
    }

    private ICExpr BuildExpression(IHoloExpr expr)
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

        if (expr is HoloIdentifier ident)
        {
            return new CIdentifier(ident.Name);
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
            var args = new List<ICExpr>();
            foreach (var arg in call.Args)
            {
                args.Add(BuildExpression(arg));
            }
            return new CCallExpr(BuildExpression(call.Callee), args);
        }

        if (expr is HoloIndexRefExpr indexRef)
        {
            return new CIndexRefExpr(
                BuildExpression(indexRef.Source),
                BuildExpression(indexRef.Index)
            );
        }

        if (expr is HoloIfExpr ifExpr)
        {
            return BuildConditionalOperator(ifExpr);
        }

        if (expr is HoloCollectionExpr col)
        {
            var elements = new List<ICExpr>();
            foreach (var e in col.elements)
            {
                elements.Add(BuildExpression(e));
            }
            return new CCollectionExpr(elements);
        }

        // TODO: ブロック式のサポート

        //if (expr is HoloBlockExpr blockExpr)
        //{
        //    var exprs = new List<ICExpr>();
        //    foreach (var e in blockExpr.Expressions)
        //    {
        //        exprs.Add(BuildExpression(e));
        //    }
        //    return new CStmtExpr(exprs);
        //}

        throw new NotSupportedException($"Unsupported expression: {expr.GetType().Name}");
    }

    private ICExpr BuildConditionalOperator(HoloIfExpr ifExpr)
    {
        var cond = BuildExpression(ifExpr.condition);
        var thenExpr = BuildExpression(ifExpr.thenExpr);
        var elseExpr = BuildExpression(ifExpr.elseExpr);

        return new CConditionalOperator(cond, thenExpr, elseExpr);
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

    // NOTE: includeAddはヘッダーと実装の両方で使っている場合でも、ヘッダー側に追加していれば十分であるためヘッダーのみ指定する。
    private string MapType(HoloNamedType holoType, IncludeAdd includeAdd)
    {
        string holoTypeName = holoType.name;

        // holo言語のエイリアス
        switch (holoTypeName)
        {
            case "int":
                holoTypeName = "int32";
                break;

            case "uint":
                holoTypeName = "uint32";
                break;

            case "byte":
                holoTypeName = "uint8";
                break;
        }

        string cType;

        // C言語の型にマッピング
        switch (holoTypeName)
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
                cType = holoTypeName;
                break; // その他の型はそのまま使う（ユーザー定義型など）
        }

        // include追加

        if (C99Feature)
        {
            var stdIntTypes = new[] { "int8_t", "int16_t", "int32_t", "int64_t", "uint8_t", "uint16_t", "uint32_t", "uint64_t" };

            if (stdIntTypes.Contains(cType))
            {
                AddInclude("<stdint.h>", includeAdd);
            }

            if (cType == "bool")
            {
                AddInclude("<stdbool.h>", includeAdd);
            }
        }

        return cType;
    }
}
