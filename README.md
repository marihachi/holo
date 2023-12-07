# holo
A programming language refined the C.

WIP!

## Concept
- コンパイルするとC言語のコードが生成される
- C言語ライクでありながら改良された文法
  - if式やswitch式のサポート
  - ブロック式による値のReturn
- 高級な型システム
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
