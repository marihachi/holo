namespace Holoc.Compile.C;

public enum CVersion
{
    C89,
    C99,
    C11,
    C17,
    C23,
};


public record CFile(List<string> Includes, List<ICDecl> Declarations);

public interface ICDecl;


// Top-level declarations

public record CFunctionDecl(string ReturnType, string Name, List<CParam> Parameters, CBlock? Body) : ICDecl;

public record CParam(string Type, string Name);

public record CBlock(List<ICStmt> Statements);


// Statements

public interface ICStmt;

public record CVariableDeclStmt(string Type, string Name, ICExpr? Initializer) : ICStmt, ICDecl;

public record CAssignStmt(ICExpr Target, string Op, ICExpr Value) : ICStmt;

public record CIfStmt(ICExpr Condition, CBlock Then, ICStmt? Else) : ICStmt;
// Else: null = no else, CIfStmt = else-if, CBlockStmt = else { }

public record CWhileStmt(ICExpr Condition, CBlock Body) : ICStmt;

public record CBreakStmt() : ICStmt;

public record CContinueStmt() : ICStmt;

public record CReturnStmt(ICExpr? Value) : ICStmt;

public record CExprStmt(ICExpr Expression) : ICStmt;

public record CBlockStmt(CBlock Block) : ICStmt;


// Expressions

public interface ICExpr;

public record CNumberLiteral(long Value) : ICExpr;

public record CBoolLiteral(bool Value) : ICExpr;

public record CIdentifier(string Name) : ICExpr;

public record CUnaryExpr(string Op, ICExpr Operand) : ICExpr;

public record CBinaryExpr(ICExpr Left, string Op, ICExpr Right) : ICExpr;

public record CCallExpr(ICExpr Callee, List<ICExpr> Args) : ICExpr;

public record CTernaryExpr(ICExpr Condition, ICExpr Then, ICExpr Else) : ICExpr;
