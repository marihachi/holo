# holo
A better C-like programming language.

WIP!

## Concept
- Compile to generate LLVM IR
- Improved C-based syntax
  - if式やswitch式のサポート
  - ブロック式による値のReturn
- High-level type system
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

## How to compile
```sh
# install holo
npm i
npm run build

# compile
npm run holoc ./debug/main.ho
clang ./debug/main.ll -o ./debug/main

# run
./debug/main
```

## Contributors
- [@marihachi](https://github.com/marihachi) - [Details](https://github.com/marihachi/holo/issues?q=author%3Amarihachi)
- [@ikasoba](https://github.com/ikasoba) - [Details](https://github.com/marihachi/holo/issues?q=author%3Aikasoba)

## License
MIT License
