using Holoc.Compile.Syntax;
using Holoc.Compile.Syntax.Node;
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
            Assert.Equal(NodeKind.FunctionDecl, functionDecl.Kind);
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
            Assert.Equal(NodeKind.FunctionDecl, functionDecl.Kind);

            Assert.NotNull(functionDecl.Operands);
            Assert.Equal(2, functionDecl.Operands.Count);

            var param = functionDecl.Operands[0];
            Assert.NotNull(param);
            Assert.Equal(NodeKind.FunctionParameter, param.Kind);
            Assert.Equal("a", param.Name);

            param = functionDecl.Operands[1];
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
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("fn abc(a: int, b: int) { }"));
            using var reader = new StreamReader(stream);

            var result = Parser.Parse(reader);

            Assert.NotNull(result);
            Assert.Equal(NodeKind.Unit, result.Kind);
            Assert.NotNull(result.Body);
            Assert.Single(result.Body);

            var functionDecl = result.Body[0];
            Assert.Equal(NodeKind.FunctionDecl, functionDecl.Kind);

            Assert.NotNull(functionDecl.Operands);
            Assert.Equal(2, functionDecl.Operands.Count);

            var param = functionDecl.Operands[0];
            Assert.NotNull(param);
            Assert.Equal(NodeKind.FunctionParameter, param.Kind);
            Assert.Equal("a", param.Name);
            Assert.NotNull(param.Operands);
            Assert.Single(param.Operands);

            var paramType = param.Operands[0];
            Assert.NotNull(paramType);
            Assert.Equal(NodeKind.TypeReference, paramType.Kind);
            Assert.Equal("int", paramType.Name);

            param = functionDecl.Operands[0];
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
    }
}
