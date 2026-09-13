using Holoc.Compile.C;
using Holoc.Compile.Holo;
using Holoc.Compile.Syntax;
using System.Text;

namespace Holoc.Compile.Tests
{
    /// <summary>
    /// holo言語のソースからC言語のソースへの変換を通しで検証します。
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
        /// main関数にはstaticを付けない
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
            // holo側でintと書いていても、int32_tではなくintを出力します。
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
        /// exportが付いたmain関数にもstaticを付けない
        /// </summary>
        [Fact]
        public void ExportedMainFunctionIsNotStaticTest()
        {
            var result = Emit("export fn main(): int { return 0; }");

            Assert.Contains("int main(void)", result.Impl);
            Assert.DoesNotContain("static", result.Impl);
        }

        /// <summary>
        /// 実装ファイル中の出現位置を返します。
        /// </summary>
        private static int IndexOf(string impl, string text)
        {
            var index = impl.IndexOf(text, StringComparison.Ordinal);
            Assert.True(index >= 0, $"'{text}' が出力に含まれていません。");
            return index;
        }

        /// <summary>
        /// 後で定義される関数の前方宣言を、呼び出し元より前に出力する
        /// </summary>
        [Fact]
        public void ForwardFunctionDeclIsEmittedTest()
        {
            // addはmainより後に定義されています。
            var result = Emit("fn main(): int { return add(1, 2); } fn add(a: int, b: int): int { return a + b; }");

            var protoIndex = IndexOf(result.Impl, "int32_t add(int32_t a, int32_t b);");
            var defIndex = IndexOf(result.Impl, "int32_t add(int32_t a, int32_t b) {");

            Assert.True(protoIndex < defIndex, "前方宣言が定義より後にあります。");

            var callerIndex = IndexOf(result.Impl, "int main(void) {");
            Assert.True(protoIndex < callerIndex, "前方宣言が呼び出し元の定義より後にあります。");
        }

        /// <summary>
        /// グローバル変数の前方宣言を出力する
        /// </summary>
        [Fact]
        public void ForwardVariableDeclIsEmittedTest()
        {
            var result = Emit("var g: int = 1; fn f(): int { return g; }");

            var protoIndex = IndexOf(result.Impl, "static int32_t g;");
            var defIndex = IndexOf(result.Impl, "static int32_t g = 1;");

            Assert.True(protoIndex < defIndex, "前方宣言が定義より後にあります。");
        }

        /// <summary>
        /// 前方宣言は変数、関数の順に並べ、全ての定義より前に出力する
        /// </summary>
        [Fact]
        public void ForwardDeclsArePlacedBeforeAllDefinitionsTest()
        {
            var result = Emit("var g: int = 1; fn f(): int { return g; } fn main(): int { return f(); }");

            var varProtoIndex = IndexOf(result.Impl, "static int32_t g;");
            var funcProtoIndex = IndexOf(result.Impl, "int32_t f(void);");
            var firstDefIndex = IndexOf(result.Impl, "static int32_t g = 1;");

            Assert.True(varProtoIndex < funcProtoIndex, "変数の前方宣言が関数の前方宣言より後にあります。");
            Assert.True(funcProtoIndex < firstDefIndex, "前方宣言が最初の定義より後にあります。");
        }

        /// <summary>
        /// 前方宣言はヘッダーの内容に影響しない
        /// </summary>
        [Fact]
        public void ForwardDeclsDoNotAffectHeaderTest()
        {
            var result = Emit("var g: int = 1; fn f(): int { return g; }");

            // exportが付いていないため、ヘッダーには出力しません。
            Assert.DoesNotContain("g", result.Header);
            Assert.DoesNotContain("f(void)", result.Header);
        }

        /// <summary>
        /// exportされた宣言も実装ファイルに前方宣言を出力する
        /// </summary>
        [Fact]
        public void ExportedDeclsHaveForwardDeclInImplTest()
        {
            var result = Emit("export var g: int = 1; export fn f(): int { return g; }");

            var varProtoIndex = IndexOf(result.Impl, "int32_t g;");
            var varDefIndex = IndexOf(result.Impl, "int32_t g = 1;");
            Assert.True(varProtoIndex < varDefIndex, "前方宣言が定義より後にあります。");

            Assert.Contains("int32_t f(void);", result.Impl);

            // ヘッダー側の変数はexternで宣言します。
            Assert.Contains("extern int32_t g;", result.Header);
            Assert.Contains("int32_t f(void);", result.Header);
        }

        /// <summary>
        /// 非exportの関数は前方宣言にもstaticを付ける
        /// </summary>
        [Fact]
        public void NonExportedFunctionForwardDeclIsStaticTest()
        {
            // 前方宣言と定義でリンケージが異なるのは未定義動作です (C11 6.2.2p7)。
            var result = Emit("fn main(): int { return f(); } fn f(): int { return 0; }");

            Assert.Contains("static int32_t f(void);", result.Impl);
        }

        /// <summary>
        /// 非exportのグローバル変数は前方宣言にもstaticを付ける
        /// </summary>
        [Fact]
        public void NonExportedVariableForwardDeclIsStaticTest()
        {
            // 前方宣言と定義でリンケージが異なるのは未定義動作です (C11 6.2.2p7)。
            var result = Emit("var g: int = 1; fn f(): int { return g; }");

            Assert.Contains("static int32_t g;", result.Impl);
            Assert.DoesNotContain("extern int32_t g;", result.Impl);
        }

        /// <summary>
        /// exportされた関数は前方宣言にstaticを付けない
        /// </summary>
        [Fact]
        public void ExportedFunctionForwardDeclIsNotStaticTest()
        {
            var result = Emit("export fn f(): int { return 0; }");

            Assert.Contains("int32_t f(void);", result.Impl);
            Assert.DoesNotContain("static", result.Impl);
        }

        /// <summary>
        /// main関数は前方宣言にstaticを付けない
        /// </summary>
        [Fact]
        public void MainFunctionForwardDeclIsNotStaticTest()
        {
            var result = Emit("fn main(): int { return 0; }");

            Assert.Contains("int main(void);", result.Impl);
            Assert.DoesNotContain("static", result.Impl);
        }
    }
}
