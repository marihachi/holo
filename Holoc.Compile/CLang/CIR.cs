namespace Holoc.Compile.CLang;

public record CUnit(List<string> Includes, List<CFunctionDecl> Declarations);

public record CFunctionDecl(string ReturnType, string Name, List<CParam> Parameters, CBlock? Body);
// Body == null means extern declaration

public record CParam(string Type, string Name);

public record CBlock(List<CStmt> Statements);


// Statements

public abstract record CStmt;

public record CVariableDeclStmt(string Type, string Name, CExpr? Initializer) : CStmt;

public record CAssignStmt(CExpr Target, string Op, CExpr Value) : CStmt;

public record CIfStmt(CExpr Condition, CBlock Then, CStmt? Else) : CStmt;
// Else: null = no else, CIfStmt = else-if, CBlockStmt = else { }

public record CWhileStmt(CExpr Condition, CBlock Body) : CStmt;

public record CBreakStmt() : CStmt;

public record CContinueStmt() : CStmt;

public record CReturnStmt(CExpr? Value) : CStmt;

public record CExprStmt(CExpr Expression) : CStmt;

public record CBlockStmt(CBlock Block) : CStmt;


// Expressions

public abstract record CExpr;

public record CNumberLiteral(object Value) : CExpr;

public record CIdentifier(string Name) : CExpr;

public record CUnaryExpr(string Op, CExpr Operand) : CExpr;

public record CBinaryExpr(CExpr Left, string Op, CExpr Right) : CExpr;

public record CCallExpr(CExpr Callee, List<CExpr> Args) : CExpr;

public record CTernaryExpr(CExpr Condition, CExpr Then, CExpr Else) : CExpr;

public record CStmtExpr(List<CExpr> Expressions) : CExpr; // GCC: ({ expr; ... })
