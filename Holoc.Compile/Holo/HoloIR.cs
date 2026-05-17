namespace Holoc.Compile.Holo;

public interface IHoloDecl;

public record HoloUnit(string fileName, List<IHoloDecl> Declarations);


// Top-level declarations

public record HoloFunctionDecl(
    string Name,
    string ReturnType,
    List<HoloParam> Parameters,
    List<IHoloStmt>? Body    // null = extern
) : IHoloDecl;

public record HoloParam(string Name, string Type);


// Statements

public interface IHoloStmt;

public record HoloVariableDeclStmt(string Name, string Type, IHoloExpr? Initializer) : IHoloStmt, IHoloDecl;

public record HoloAssignStmt(IHoloExpr Target, HoloAssignOp Op, IHoloExpr Value) : IHoloStmt;

public record HoloIfStmt(IHoloExpr Condition, List<IHoloStmt> Then, IHoloStmt? Else) : IHoloStmt;
// Else: null = no else, HoloIfStmt = else-if, HoloBlockStmt = else { }

public record HoloWhileStmt(IHoloExpr Condition, List<IHoloStmt> Body) : IHoloStmt;

public record HoloBreakStmt() : IHoloStmt;

public record HoloContinueStmt() : IHoloStmt;

public record HoloReturnStmt(IHoloExpr? Value) : IHoloStmt;

public record HoloExprStmt(IHoloExpr Expression) : IHoloStmt;

public record HoloBlockStmt(List<IHoloStmt> Block) : IHoloStmt;


// Expressions

public interface IHoloExpr;

public record HoloNumberLiteral(long Value) : IHoloExpr;

public record HoloBoolLiteral(bool Value) : IHoloExpr;

public record HoloReference(string Name) : IHoloExpr;

public record HoloUnaryExpr(HoloUnaryOp Op, IHoloExpr Operand) : IHoloExpr;

public record HoloBinaryExpr(IHoloExpr Left, HoloBinaryOp Op, IHoloExpr Right) : IHoloExpr;

public record HoloCallExpr(IHoloExpr Callee, List<IHoloExpr> Args) : IHoloExpr;

public record HoloWhenExpr(List<HoloWhenArm> Arms) : IHoloExpr;
// Holo-specific: lowered to nested ternary in C IR

public record HoloBlockExpr(List<IHoloExpr> Expressions) : IHoloExpr;
// Holo-specific: lowered to GCC statement expression in C IR

public record HoloWhenArm(IHoloExpr? Condition, IHoloExpr Value);
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
