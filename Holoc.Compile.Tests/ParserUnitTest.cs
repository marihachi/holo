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

            Assert.NotNull(result);
            Assert.Equal(NodeKind.Unit, result.Kind);
            Assert.NotNull(result.Body);
            Assert.Single(result.Body);

            var functionDecl = result.Body[0];
            Assert.Equal(NodeKind.FunctionDeclaration, functionDecl.Kind);
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

            Assert.NotNull(result);
            Assert.Equal(NodeKind.Unit, result.Kind);
            Assert.NotNull(result.Body);
            Assert.Single(result.Body);

            var functionDecl = result.Body[0];
            Assert.Equal(NodeKind.FunctionDeclaration, functionDecl.Kind);

            Assert.NotNull(functionDecl.Parameters);
            Assert.Equal(2, functionDecl.Parameters.Count);

            var param = functionDecl.Parameters[0];
            Assert.NotNull(param);
            Assert.Equal(NodeKind.FunctionParameter, param.Kind);
            Assert.Equal("a", param.Name);

            param = functionDecl.Parameters[1];
            Assert.NotNull(param);
            Assert.Equal(NodeKind.FunctionParameter, param.Kind);
            Assert.Equal("b", param.Name);
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

            Assert.NotNull(result);
            Assert.Equal(NodeKind.Unit, result.Kind);
            Assert.NotNull(result.Body);
            Assert.Single(result.Body);

            var functionDecl = result.Body[0];
            Assert.Equal(NodeKind.FunctionDeclaration, functionDecl.Kind);

            var returnType = functionDecl.Operands?[0];
            Assert.NotNull(returnType);
            Assert.Equal(NodeKind.TypeReference, returnType.Kind);
            Assert.Equal("int", returnType.Name);

            Assert.NotNull(functionDecl.Parameters);
            Assert.Equal(2, functionDecl.Parameters.Count);

            var param = functionDecl.Parameters[0];
            Assert.NotNull(param);
            Assert.Equal(NodeKind.FunctionParameter, param.Kind);
            Assert.Equal("a", param.Name);
            Assert.NotNull(param.Operands);
            Assert.Single(param.Operands);

            var paramType = param.Operands[0];
            Assert.NotNull(paramType);
            Assert.Equal(NodeKind.TypeReference, paramType.Kind);
            Assert.Equal("int", paramType.Name);

            param = functionDecl.Parameters[0];
            Assert.NotNull(param);
            Assert.Equal(NodeKind.FunctionParameter, param.Kind);
            Assert.Equal("a", param.Name);
            Assert.NotNull(param.Operands);
            Assert.Single(param.Operands);

            paramType = param.Operands[0];
            Assert.NotNull(paramType);
            Assert.Equal(NodeKind.TypeReference, paramType.Kind);
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

            Assert.NotNull(result);
            Assert.Equal(NodeKind.Unit, result.Kind);
            Assert.NotNull(result.Body);
            Assert.Single(result.Body);

            var variableDecl = result.Body[0];
            Assert.Equal(NodeKind.VariableDeclaration, variableDecl.Kind);
            Assert.Equal("x", variableDecl.Name);

            Assert.NotNull(variableDecl.Operands);
            Assert.Equal(2, variableDecl.Operands.Count);

            var variableType = variableDecl.Operands[0];
            Assert.NotNull(variableType);
            Assert.Equal(NodeKind.TypeReference, variableType.Kind);
            Assert.Equal("int", variableType.Name);

            var initializer = variableDecl.Operands[1];
            Assert.NotNull(initializer);
            Assert.Equal(NodeKind.NumberLiteral, initializer.Kind);
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

            Assert.NotNull(result);
            Assert.Equal(NodeKind.Unit, result.Kind);
            Assert.NotNull(result.Body);
            Assert.Single(result.Body);

            var variableDecl = result.Body[0];
            Assert.Equal(NodeKind.VariableDeclaration, variableDecl.Kind);
            Assert.Equal("x", variableDecl.Name);

            Assert.NotNull(variableDecl.Operands);
            Assert.Equal(2, variableDecl.Operands.Count);

            var variableType = variableDecl.Operands[0];
            Assert.NotNull(variableType);
            Assert.Equal(NodeKind.TypeReference, variableType.Kind);
            Assert.Equal("int", variableType.Name);

            var initializer = variableDecl.Operands[1];
            Assert.Null(initializer);
        }
    }
}
