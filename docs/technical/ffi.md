## FFI
- `ExportC`、`ImportC`は、Cのプロトタイプ宣言または変数のextern宣言をholoの構文を使って記述する。
- `ExportC`、`ImportC`は、Cで使用できる型以外は使用できない。

```
// holoの関数や変数をCに公開する

[ExportC]
var x: int;

[ExportC]
fn add(a: int, b: int): int {
  return a + b;
}
```

```
// Cの関数や変数をholoから使う

[ImportC]
declare var y: int;

[ImportC]
declare fn sub(a: int, b: int): int;
```
