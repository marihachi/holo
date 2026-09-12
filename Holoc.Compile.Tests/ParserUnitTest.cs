using Holoc.Compile.Holo.Syntax.Node;
using Holoc.Compile.Syntax;
using System.Text;

namespace Holoc.Compile.Tests
{
    public class ParserUnitTest
    {
        public Parser Parser = new();

        /// <summary>
        /// 関数宣言
        /// </summary>
        [Fact]
        public void FunctionTest()
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("fn abc() { }"));
            using var reader = new StreamReader(stream);

            var result = Parser.Parse(reader);

            var unit = Assert.IsType<SyntaxUnit>(result);
            Assert.Single(unit.Body);

            var functionDecl = Assert.IsType<SyntaxFunctionDecl>(unit.Body[0]);
            Assert.Equal("abc", functionDecl.Name);
        }

        /// <summary>
        /// 型指定なしの引数を持つ関数宣言
        /// </summary>
        [Fact]
        public void FunctionNoTypeParamsTest()
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("fn abc(a, b) { }"));
            using var reader = new StreamReader(stream);

            var result = Parser.Parse(reader);

            var unit = Assert.IsType<SyntaxUnit>(result);
            Assert.Single(unit.Body);

            var functionDecl = Assert.IsType<SyntaxFunctionDecl>(unit.Body[0]);

            Assert.NotNull(functionDecl.Parameters);
            Assert.Equal(2, functionDecl.Parameters.Count);

            var param = Assert.IsType<SyntaxFunctionParameter>(functionDecl.Parameters[0]);
            Assert.Equal("a", param.Name);
            Assert.Null(param.ParamType);

            param = Assert.IsType<SyntaxFunctionParameter>(functionDecl.Parameters[1]);
            Assert.Equal("b", param.Name);
            Assert.Null(param.ParamType);
        }

        /// <summary>
        /// 型指定された引数を持つ関数宣言
        /// </summary>
        [Fact]
        public void FunctionParamsTest()
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("fn abc(a: int, b: int): int { }"));
            using var reader = new StreamReader(stream);

            var result = Parser.Parse(reader);

            var unit = Assert.IsType<SyntaxUnit>(result);
            Assert.Single(unit.Body);

            var functionDecl = Assert.IsType<SyntaxFunctionDecl>(unit.Body[0]);

            var returnType = Assert.IsType<SyntaxNamedType>(functionDecl.ReturnType);
            Assert.Equal("int", returnType.Name);

            Assert.NotNull(functionDecl.Parameters);
            Assert.Equal(2, functionDecl.Parameters.Count);

            var param = Assert.IsType<SyntaxFunctionParameter>(functionDecl.Parameters[0]);
            Assert.Equal("a", param.Name);

            var paramType = Assert.IsType<SyntaxNamedType>(param.ParamType);
            Assert.Equal("int", paramType.Name);

            param = Assert.IsType<SyntaxFunctionParameter>(functionDecl.Parameters[1]);
            Assert.Equal("b", param.Name);

            paramType = Assert.IsType<SyntaxNamedType>(param.ParamType);
            Assert.Equal("int", paramType.Name);
        }

        /// <summary>
        /// 初期化付きの変数宣言
        /// </summary>
        [Fact]
        public void VariableInitTest()
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("var x: int = 1;"));
            using var reader = new StreamReader(stream);

            var result = Parser.Parse(reader);

            var unit = Assert.IsType<SyntaxUnit>(result);
            Assert.Single(unit.Body);

            var variableDecl = Assert.IsType<SyntaxVariableDecl>(unit.Body[0]);
            Assert.Equal("x", variableDecl.Name);

            var variableType = Assert.IsType<SyntaxNamedType>(variableDecl.VariableType);
            Assert.Equal("int", variableType.Name);

            var initializer = Assert.IsType<SyntaxNumberLiteral>(variableDecl.Initializer);
            Assert.Equal(1L, initializer.Value);
        }

        /// <summary>
        /// 初期化なしの変数宣言
        /// </summary>
        [Fact]
        public void VariableWithoutInitTest()
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("var x: int;"));
            using var reader = new StreamReader(stream);

            var result = Parser.Parse(reader);

            var unit = Assert.IsType<SyntaxUnit>(result);
            Assert.Single(unit.Body);

            var variableDecl = Assert.IsType<SyntaxVariableDecl>(unit.Body[0]);
            Assert.Equal("x", variableDecl.Name);

            var variableType = Assert.IsType<SyntaxNamedType>(variableDecl.VariableType);
            Assert.Equal("int", variableType.Name);

            Assert.Null(variableDecl.Initializer);
        }

        /// <summary>
        /// 変数宣言の型を準備します。
        /// </summary>
        private ISyntaxNode? PrepareVariableType(string source)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(source));
            using var reader = new StreamReader(stream);

            var result = Parser.Parse(reader);

            var unit = Assert.IsType<SyntaxUnit>(result);
            Assert.Single(unit.Body);

            var variableDecl = Assert.IsType<SyntaxVariableDecl>(unit.Body[0]);
            return variableDecl.VariableType;
        }

        /// <summary>
        /// 配列型
        /// </summary>
        [Fact]
        public void ArrayTypeTest()
        {
            // int[3] -> 要素数3のint配列
            var collectionType = Assert.IsType<SyntaxCollectionType>(PrepareVariableType("var x: int[3];"));
            Assert.Equal(3L, collectionType.Size);

            var elementType = Assert.IsType<SyntaxNamedType>(collectionType.ElementType);
            Assert.Equal("int", elementType.Name);
        }

        /// <summary>
        /// 要素数の指定がない配列型
        /// </summary>
        [Fact]
        public void ArrayTypeWithoutSizeTest()
        {
            // int[] -> 要素数の指定がないint配列
            var collectionType = Assert.IsType<SyntaxCollectionType>(PrepareVariableType("var x: int[];"));
            Assert.Null(collectionType.Size);

            var elementType = Assert.IsType<SyntaxNamedType>(collectionType.ElementType);
            Assert.Equal("int", elementType.Name);
        }

        /// <summary>
        /// ポインタ型
        /// </summary>
        [Fact]
        public void PointerTypeTest()
        {
            // int* -> intへのポインタ
            var pointerType = Assert.IsType<SyntaxPointerType>(PrepareVariableType("var x: int*;"));

            var elementType = Assert.IsType<SyntaxNamedType>(pointerType.ElementType);
            Assert.Equal("int", elementType.Name);
        }

        /// <summary>
        /// ポインタの配列型
        /// </summary>
        [Fact]
        public void PointerArrayTypeTest()
        {
            // int*[3] -> intへのポインタ3個の配列
            var collectionType = Assert.IsType<SyntaxCollectionType>(PrepareVariableType("var x: int*[3];"));
            Assert.Equal(3L, collectionType.Size);

            var pointerType = Assert.IsType<SyntaxPointerType>(collectionType.ElementType);

            var elementType = Assert.IsType<SyntaxNamedType>(pointerType.ElementType);
            Assert.Equal("int", elementType.Name);
        }

        /// <summary>
        /// 配列へのポインタ型
        /// </summary>
        [Fact]
        public void ArrayPointerTypeTest()
        {
            // int[3]* -> int3個の配列へのポインタ
            var pointerType = Assert.IsType<SyntaxPointerType>(PrepareVariableType("var x: int[3]*;"));

            var collectionType = Assert.IsType<SyntaxCollectionType>(pointerType.ElementType);
            Assert.Equal(3L, collectionType.Size);

            var elementType = Assert.IsType<SyntaxNamedType>(collectionType.ElementType);
            Assert.Equal("int", elementType.Name);
        }

        /// <summary>
        /// 型の修飾子は後置でのみ指定できる
        /// </summary>
        [Fact]
        public void PrefixTypeModifierIsErrorTest()
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("var x: [3]int;"));
            using var reader = new StreamReader(stream);

            var result = Parser.Parse(reader);

            Assert.Null(result);
            Assert.NotEmpty(Parser.Errors);
        }

        /// <summary>
        /// 型名を2つ続けて指定することはできない
        /// </summary>
        [Fact]
        public void DuplicatedTypeNameIsErrorTest()
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("var x: int int;"));
            using var reader = new StreamReader(stream);

            var result = Parser.Parse(reader);

            Assert.Null(result);
            Assert.NotEmpty(Parser.Errors);
        }

        /// <summary>
        /// 式を準備します。
        /// </summary>
        private ISyntaxNode PrepareExpression(string expression)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes($"var x: int = {expression};"));
            using var reader = new StreamReader(stream);

            var result = Parser.Parse(reader);

            var unit = Assert.IsType<SyntaxUnit>(result);
            Assert.Single(unit.Body);

            var variableDecl = Assert.IsType<SyntaxVariableDecl>(unit.Body[0]);
            Assert.NotNull(variableDecl.Initializer);
            return variableDecl.Initializer;
        }

        /// <summary>
        /// 代入文を準備します。
        /// </summary>
        private SyntaxAssignmentStatement PrepareAssignment(string statement)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes($"fn f() {{ {statement} }}"));
            using var reader = new StreamReader(stream);

            var result = Parser.Parse(reader);

            var unit = Assert.IsType<SyntaxUnit>(result);
            Assert.Single(unit.Body);

            var functionDecl = Assert.IsType<SyntaxFunctionDecl>(unit.Body[0]);
            Assert.NotNull(functionDecl.Body);
            Assert.Single(functionDecl.Body);

            return Assert.IsType<SyntaxAssignmentStatement>(functionDecl.Body[0]);
        }

        /// <summary>
        /// 二項演算子
        /// </summary>
        [Theory]
        [InlineData("+", NodeMode.Add)]
        [InlineData("-", NodeMode.Sub)]
        [InlineData("*", NodeMode.Mul)]
        [InlineData("/", NodeMode.Div)]
        [InlineData("%", NodeMode.Rem)]
        [InlineData("<<", NodeMode.ShiftLeft)]
        [InlineData(">>", NodeMode.ShiftRight)]
        [InlineData("&", NodeMode.BitAnd)]
        [InlineData("|", NodeMode.BitOr)]
        [InlineData("^", NodeMode.Xor)]
        [InlineData(">", NodeMode.Gt)]
        [InlineData("<", NodeMode.Lt)]
        [InlineData(">=", NodeMode.GtEq)]
        [InlineData("<=", NodeMode.LtEq)]
        [InlineData("==", NodeMode.Eq)]
        [InlineData("!=", NodeMode.NotEq)]
        public void BinaryOperatorTest(string op, NodeMode expectedMode)
        {
            var binaryOperation = Assert.IsType<SyntaxBinaryOperation>(PrepareExpression($"1 {op} 2"));
            Assert.Equal(expectedMode, binaryOperation.Mode);

            var left = Assert.IsType<SyntaxNumberLiteral>(binaryOperation.Left);
            Assert.Equal(1L, left.Value);

            var right = Assert.IsType<SyntaxNumberLiteral>(binaryOperation.Right);
            Assert.Equal(2L, right.Value);
        }

        /// <summary>
        /// 複合代入演算子
        /// </summary>
        [Theory]
        [InlineData("=", NodeMode.None)]
        [InlineData("+=", NodeMode.Add)]
        [InlineData("-=", NodeMode.Sub)]
        [InlineData("*=", NodeMode.Mul)]
        [InlineData("/=", NodeMode.Div)]
        [InlineData("%=", NodeMode.Rem)]
        [InlineData("<<=", NodeMode.ShiftLeft)]
        [InlineData(">>=", NodeMode.ShiftRight)]
        [InlineData("&=", NodeMode.BitAnd)]
        [InlineData("|=", NodeMode.BitOr)]
        [InlineData("^=", NodeMode.Xor)]
        public void AssignmentOperatorTest(string op, NodeMode expectedMode)
        {
            var assignment = PrepareAssignment($"x {op} 1;");
            Assert.Equal(expectedMode, assignment.Mode);

            var target = Assert.IsType<SyntaxReference>(assignment.Target);
            Assert.Equal("x", target.Name);

            var expression = Assert.IsType<SyntaxNumberLiteral>(assignment.Expression);
            Assert.Equal(1L, expression.Value);
        }

        /// <summary>
        /// ビット演算子とシフト演算子の優先順位はC言語と同じ
        /// </summary>
        [Fact]
        public void BitwiseOperatorPrecedenceTest()
        {
            // 1 | 2 ^ 3 & 4 == 5 << 6 + 7 * 8
            //   -> 1 | (2 ^ (3 & (4 == (5 << (6 + (7 * 8))))))
            var bitOr = Assert.IsType<SyntaxBinaryOperation>(PrepareExpression("1 | 2 ^ 3 & 4 == 5 << 6 + 7 * 8"));
            Assert.Equal(NodeMode.BitOr, bitOr.Mode);

            var xor = Assert.IsType<SyntaxBinaryOperation>(bitOr.Right);
            Assert.Equal(NodeMode.Xor, xor.Mode);

            var bitAnd = Assert.IsType<SyntaxBinaryOperation>(xor.Right);
            Assert.Equal(NodeMode.BitAnd, bitAnd.Mode);

            var eq = Assert.IsType<SyntaxBinaryOperation>(bitAnd.Right);
            Assert.Equal(NodeMode.Eq, eq.Mode);

            var shiftLeft = Assert.IsType<SyntaxBinaryOperation>(eq.Right);
            Assert.Equal(NodeMode.ShiftLeft, shiftLeft.Mode);

            var add = Assert.IsType<SyntaxBinaryOperation>(shiftLeft.Right);
            Assert.Equal(NodeMode.Add, add.Mode);

            var mul = Assert.IsType<SyntaxBinaryOperation>(add.Right);
            Assert.Equal(NodeMode.Mul, mul.Mode);
        }

        /// <summary>
        /// 剰余演算子の優先順位は乗算・除算と同じ
        /// </summary>
        [Fact]
        public void RemainderOperatorPrecedenceTest()
        {
            // 1 + 2 % 3 -> 1 + (2 % 3)
            var add = Assert.IsType<SyntaxBinaryOperation>(PrepareExpression("1 + 2 % 3"));
            Assert.Equal(NodeMode.Add, add.Mode);

            var rem = Assert.IsType<SyntaxBinaryOperation>(add.Right);
            Assert.Equal(NodeMode.Rem, rem.Mode);

            // 1 % 2 * 3 -> (1 % 2) * 3
            var mul = Assert.IsType<SyntaxBinaryOperation>(PrepareExpression("1 % 2 * 3"));
            Assert.Equal(NodeMode.Mul, mul.Mode);

            rem = Assert.IsType<SyntaxBinaryOperation>(mul.Left);
            Assert.Equal(NodeMode.Rem, rem.Mode);
        }

        /// <summary>
        /// 前置演算子は二項演算子より強く結合する
        /// </summary>
        [Fact]
        public void PrefixOperatorPrecedenceTest()
        {
            // -1 * 2 -> (-1) * 2
            var mul = Assert.IsType<SyntaxBinaryOperation>(PrepareExpression("-1 * 2"));
            Assert.Equal(NodeMode.Mul, mul.Mode);

            var unaryOperation = Assert.IsType<SyntaxUnaryOperation>(mul.Left);
            Assert.Equal(NodeMode.Sub, unaryOperation.Mode);

            var operand = Assert.IsType<SyntaxNumberLiteral>(unaryOperation.Expression);
            Assert.Equal(1L, operand.Value);
        }
    }
}
