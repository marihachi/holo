using System.Text;

namespace Holoc.Compile.CLang;

public class CEmitter
{
    private readonly StringBuilder _sb = new();
    private int _indent;
    private string _indentStr = "";

    public string Emit(CUnit unit)
    {
        _sb.Clear();
        _indent = 0;
        _indentStr = "";

        foreach (var include in unit.Includes)
            Write($"#include {include}\n");
        if (unit.Includes.Count > 0)
            Write("\n");
        foreach (var decl in unit.Declarations)
            EmitFunctionDecl(decl);

        return _sb.ToString();
    }

    private void PushIndent()
    {
        _indentStr = new string(' ', ++_indent * 4);
    }

    private void PopIndent()
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

    private void WriteIndented(string s)
    {
        _sb.Append(_indentStr);
        _sb.Append(s);
    }

    // --- Declarations ---

    private void EmitFunctionDecl(CFunctionDecl decl)
    {
        var prefix = decl.Body == null ? "extern " : "";
        Write($"{prefix}{decl.ReturnType} {decl.Name}(");
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
        WriteIndented("{\n");
        PushIndent();
        foreach (var stmt in block.Statements)
        {
            EmitStatement(stmt);
        }
        PopIndent();
        WriteIndented("}\n");
    }

    private void EmitStatement(CStmt stmt)
    {
        switch (stmt)
        {
            case CVariableDeclStmt s:
                WriteIndented($"{s.Type} {s.Name}");
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
                WriteIndented("while (");
                EmitExpression(s.Condition);
                Write(")\n");
                EmitBlock(s.Body);
                break;

            case CBreakStmt:
                WriteIndented("break;\n");
                break;

            case CContinueStmt:
                WriteIndented("continue;\n");
                break;

            case CReturnStmt s:
                WriteIndented("return");
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
        WriteIndented("if (");
        EmitExpression(stmt.Condition);
        Write(")\n");
        EmitBlock(stmt.Then);
        EmitElseChain(stmt.Else);
    }

    private void EmitElseChain(CStmt? elseStmt)
    {
        if (elseStmt == null) return;

        if (elseStmt is CIfStmt elseIf)
        {
            WriteIndented("else if (");
            EmitExpression(elseIf.Condition);
            Write(")\n");
            EmitBlock(elseIf.Then);
            EmitElseChain(elseIf.Else);
        }
        else
        {
            WriteIndented("else\n");
            if (elseStmt is CBlockStmt b)
                EmitBlock(b.Block);
            else
                EmitStatement(elseStmt);
        }
    }

    // --- Expressions ---

    private void EmitExpression(CExpr expr)
    {
        switch (expr)
        {
            case CNumberLiteral e:
                Write(e.Value.ToString()!);
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

            case CStmtExpr e:
                Write("({ ");
                foreach (var ex in e.Expressions)
                {
                    EmitExpression(ex);
                    Write("; ");
                }
                Write("})");
                break;

            default:
                throw new NotSupportedException($"Unsupported expression: {expr.GetType().Name}");
        }
    }
}
