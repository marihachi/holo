# holo
A programming language refined the C.

WIP!

## Concept
- Compile to generate C code - コンパイルするとC言語のコードを生成する
- Improved C-based syntax - C言語をベースにした改良された文法
  - if式やswitch式のサポート
  - ブロック式による値のReturn
- High-level type system - 高級な型システム
  - 参照型のサポート
  - いずれかの型であることを表すユニオン型のサポート

## Syntax image
```
var x: int;

fn main(argc: int, argv: char*[]): int {
  var a: int;
  a = 1;

  return 0;
}
```

## License
MIT License
