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
        /// 変数宣言の型を取得します。
        /// </summary>
        private ISyntaxNode? ParseVariableType(string source)
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
            var collectionType = Assert.IsType<SyntaxCollectionType>(ParseVariableType("var x: int[3];"));
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
            var collectionType = Assert.IsType<SyntaxCollectionType>(ParseVariableType("var x: int[];"));
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
            var pointerType = Assert.IsType<SyntaxPointerType>(ParseVariableType("var x: int*;"));

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
            var collectionType = Assert.IsType<SyntaxCollectionType>(ParseVariableType("var x: int*[3];"));
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
            var pointerType = Assert.IsType<SyntaxPointerType>(ParseVariableType("var x: int[3]*;"));

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
    }
}
