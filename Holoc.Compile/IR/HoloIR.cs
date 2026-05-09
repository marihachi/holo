namespace Holoc.Compile.IR;

public record HoloUnit(List<HoloFunctionDecl> Functions);

public record HoloFunctionDecl(
    string Name,
    string ReturnType,
    List<HoloParam> Parameters,
    HoloBlock? Body    // null = extern
);

public record HoloParam(string Name, string Type);

public record HoloBlock(List<HoloStmt> Statements);


// Statements

public abstract record HoloStmt;

public record HoloVariableDeclStmt(string Name, string Type, HoloExpr? Initializer) : HoloStmt;

public record HoloAssignStmt(HoloExpr Target, HoloAssignOp Op, HoloExpr Value) : HoloStmt;

public record HoloIfStmt(HoloExpr Condition, HoloBlock Then, HoloStmt? Else) : HoloStmt;
// Else: null = no else, HoloIfStmt = else-if, HoloBlockStmt = else { }

public record HoloWhileStmt(HoloExpr Condition, HoloBlock Body) : HoloStmt;

public record HoloBreakStmt() : HoloStmt;

public record HoloContinueStmt() : HoloStmt;

public record HoloReturnStmt(HoloExpr? Value) : HoloStmt;

public record HoloExprStmt(HoloExpr Expression) : HoloStmt;

public record HoloBlockStmt(HoloBlock Block) : HoloStmt;


// Expressions

public abstract record HoloExpr;

public record HoloNumberLiteral(long Value) : HoloExpr;

public record HoloBoolLiteral(bool Value) : HoloExpr;

public record HoloReference(string Name) : HoloExpr;

public record HoloUnaryExpr(HoloUnaryOp Op, HoloExpr Operand) : HoloExpr;

public record HoloBinaryExpr(HoloExpr Left, HoloBinaryOp Op, HoloExpr Right) : HoloExpr;

public record HoloCallExpr(HoloExpr Callee, List<HoloExpr> Args) : HoloExpr;

public record HoloWhenExpr(List<HoloWhenArm> Arms) : HoloExpr;
// Holo-specific: lowered to nested ternary in C IR

public record HoloBlockExpr(List<HoloExpr> Expressions) : HoloExpr;
// Holo-specific: lowered to GCC statement expression in C IR

public record HoloWhenArm(HoloExpr? Condition, HoloExpr Value);
// Condition == null means default arm


// Operators

public enum HoloUnaryOp { Neg, Pos }

public enum HoloBinaryOp
{
    Add, Sub, Mul, Div, Rem,
    ShiftLeft, ShiftRight,
    BitAnd, BitOr, Xor,
    Gt, Lt, GtEq, LtEq, Eq, NotEq,
}

public enum HoloAssignOp
{
    None,
    Add, Sub, Mul, Div, Rem,
    BitAnd, BitOr, Xor,
    ShiftLeft, ShiftRight,
}
