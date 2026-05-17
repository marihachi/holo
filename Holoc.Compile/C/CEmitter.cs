using System.Text;

namespace Holoc.Compile.C;

public class CEmitter
{
    private readonly StringBuilder _sb = new();
    private int _indent;
    private string _indentStr = "";

    public string Emit(CFile unit)
    {
        _sb.Clear();
        _indent = 0;
        _indentStr = "";

        foreach (var include in unit.Includes)
            Write($"#include {include}\n");

        if (unit.Includes.Count > 0)
            Write("\n");
        
        foreach (var decl in unit.Declarations)
        {
            if (decl is CFunctionDecl func)
            {
                EmitFunctionDecl(func);
            }
        }

        return _sb.ToString();
    }

    private void Enter()
    {
        _indentStr = new string(' ', ++_indent * 4);
    }

    private void Leave()
    {
        _indentStr = new string(' ', --_indent * 4);
    }

    private void Write(string s)
    {
        _sb.Append(s);
    }

    private void WriteIndent()
    {
        _sb.Append(_indentStr);
    }

    private void WriteIndent(string s)
    {
        _sb.Append(_indentStr);
        _sb.Append(s);
    }

    // --- Declarations ---

    private void EmitFunctionDecl(CFunctionDecl decl)
    {
        Write($"{decl.ReturnType} {decl.Name}(");

        for (int i = 0; i < decl.Parameters.Count; i++)
        {
            if (i > 0) Write(", ");
            Write($"{decl.Parameters[i].Type} {decl.Parameters[i].Name}");
        }

        if (decl.Body == null)
        {
            Write(");\n\n");
            return;
        }

        Write(")\n");
        EmitBlock(decl.Body);
        Write("\n");
    }

    // --- Statements ---

    private void EmitBlock(CBlock block)
    {
        WriteIndent("{\n");
        Enter();
        foreach (var stmt in block.Statements)
        {
            EmitStatement(stmt);
        }
        Leave();
        WriteIndent("}\n");
    }

    private void EmitStatement(ICStmt stmt)
    {
        switch (stmt)
        {
            case CVariableDeclStmt s:
                WriteIndent($"{s.Type} {s.Name}");
                if (s.Initializer != null) { Write(" = "); EmitExpression(s.Initializer); }
                Write(";\n");
                break;

            case CAssignStmt s:
                WriteIndent();
                EmitExpression(s.Target);
                Write($" {s.Op} ");
                EmitExpression(s.Value);
                Write(";\n");
                break;

            case CIfStmt s:
                EmitIfStmt(s);
                break;

            case CWhileStmt s:
                WriteIndent("while (");
                EmitExpression(s.Condition);
                Write(")\n");
                EmitBlock(s.Body);
                break;

            case CBreakStmt:
                WriteIndent("break;\n");
                break;

            case CContinueStmt:
                WriteIndent("continue;\n");
                break;

            case CReturnStmt s:
                WriteIndent("return");
                if (s.Value != null) { Write(" "); EmitExpression(s.Value); }
                Write(";\n");
                break;

            case CExprStmt s:
                WriteIndent();
                EmitExpression(s.Expression);
                Write(";\n");
                break;

            case CBlockStmt s:
                EmitBlock(s.Block);
                break;

            default:
                throw new NotSupportedException($"Unsupported statement: {stmt.GetType().Name}");
        }
    }

    private void EmitIfStmt(CIfStmt stmt)
    {
        WriteIndent("if (");
        EmitExpression(stmt.Condition);
        Write(")\n");
        EmitBlock(stmt.Then);
        EmitElseChain(stmt.Else);
    }

    private void EmitElseChain(ICStmt? elseStmt)
    {
        if (elseStmt == null) return;

        if (elseStmt is CIfStmt elseIf)
        {
            WriteIndent("else if (");
            EmitExpression(elseIf.Condition);
            Write(")\n");
            EmitBlock(elseIf.Then);
            EmitElseChain(elseIf.Else);
        }
        else
        {
            WriteIndent("else\n");
            if (elseStmt is CBlockStmt b)
                EmitBlock(b.Block);
            else
                EmitStatement(elseStmt);
        }
    }

    // --- Expressions ---

    private void EmitExpression(ICExpr expr)
    {
        switch (expr)
        {
            case CNumberLiteral e:
                Write(e.Value.ToString()!);
                break;

            case CBoolLiteral e:
                Write(e.Value ? "true" : "false");
                break;

            case CIdentifier e:
                Write(e.Name);
                break;

            case CUnaryExpr e:
                Write($"{e.Op}(");
                EmitExpression(e.Operand);
                Write(")");
                break;

            case CBinaryExpr e:
                Write("(");
                EmitExpression(e.Left);
                Write($" {e.Op} ");
                EmitExpression(e.Right);
                Write(")");
                break;

            case CCallExpr e:
                EmitExpression(e.Callee);
                Write("(");
                for (int i = 0; i < e.Args.Count; i++)
                {
                    if (i > 0) Write(", ");
                    EmitExpression(e.Args[i]);
                }
                Write(")");
                break;

            case CTernaryExpr e:
                Write("(");
                EmitExpression(e.Condition);
                Write(" ? ");
                EmitExpression(e.Then);
                Write(" : ");
                EmitExpression(e.Else);
                Write(")");
                break;

            //case CStmtExpr e:
            //    Write("({ ");
            //    foreach (var ex in e.Expressions)
            //    {
            //        EmitExpression(ex);
            //        Write("; ");
            //    }
            //    Write("})");
            //    break;

            default:
                throw new NotSupportedException($"Unsupported expression: {expr.GetType().Name}");
        }
    }
}
