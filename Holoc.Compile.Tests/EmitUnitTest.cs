using Holoc.Compile.C;
using Holoc.Compile.Holo;
using Holoc.Compile.Syntax;
using System.Text;

namespace Holoc.Compile.Tests
{
    /// <summary>
    /// holo言語のソースからC言語のソースまでを通しで検証します。
    /// </summary>
    public class EmitUnitTest
    {
        private class EmitResult
        {
            public string Impl { get; init; } = "";
            public string Header { get; init; } = "";
        }

        /// <summary>
        /// holo言語のソースをC言語のソースに変換します。
        /// </summary>
        private EmitResult Emit(string source)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(source));
            using var reader = new StreamReader(stream);

            var parser = new Parser();
            var unitNode = parser.Parse(reader);

            Assert.Empty(parser.Errors);
            Assert.NotNull(unitNode);

            var holoIrBuilder = new HoloIRBuilder();
            holoIrBuilder.Build("a.holo", unitNode);

            var cIrBuilder = new CSyntaxNodeBuilder();
            cIrBuilder.Build(holoIrBuilder.HoloUnit);

            return new EmitResult
            {
                Impl = new CEmitter().Emit(cIrBuilder.CImpl),
                Header = new CEmitter().Emit(cIrBuilder.CHeader),
            };
        }

        /// <summary>
        /// main関数はエントリーポイントなのでstaticを付けない
        /// </summary>
        [Fact]
        public void MainFunctionIsNotStaticTest()
        {
            var result = Emit("fn main(): int { return 0; }");

            Assert.Contains("int main(void)", result.Impl);
            Assert.DoesNotContain("static", result.Impl);
        }

        /// <summary>
        /// main関数の戻り値はintになる
        /// </summary>
        [Fact]
        public void MainFunctionReturnTypeIsIntTest()
        {
            // holo側でintと書いていても、int32_tではなくintを出力する
            var result = Emit("fn main(): int { return 0; }");

            Assert.Contains("int main(void)", result.Impl);
            Assert.DoesNotContain("int32_t main", result.Impl);
        }

        /// <summary>
        /// main関数はヘッダーで公開しない
        /// </summary>
        [Fact]
        public void MainFunctionIsNotInHeaderTest()
        {
            var result = Emit("fn main(): int { return 0; }");

            Assert.DoesNotContain("main", result.Header);
        }

        /// <summary>
        /// exportが付いていない関数にはstaticを付ける
        /// </summary>
        [Fact]
        public void NonExportedFunctionIsStaticTest()
        {
            var result = Emit("fn f(): int { return 0; }");

            Assert.Contains("static int32_t f(void)", result.Impl);
            Assert.DoesNotContain("f", result.Header);
        }

        /// <summary>
        /// exportが付いた関数にはstaticを付けず、ヘッダーで公開する
        /// </summary>
        [Fact]
        public void ExportedFunctionIsNotStaticTest()
        {
            var result = Emit("export fn f(): int { return 0; }");

            Assert.Contains("int32_t f(void)", result.Impl);
            Assert.DoesNotContain("static", result.Impl);
            Assert.Contains("int32_t f(void);", result.Header);
        }

        /// <summary>
        /// mainという名前でもexportが付いていれば矛盾しない
        /// </summary>
        [Fact]
        public void ExportedMainFunctionIsNotStaticTest()
        {
            var result = Emit("export fn main(): int { return 0; }");

            Assert.Contains("int main(void)", result.Impl);
            Assert.DoesNotContain("static", result.Impl);
        }
    }
}
