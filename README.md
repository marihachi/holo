# holo
A system programming language of modern syntaxes with LLVM.

WIP!

## Concept
- Modern syntax introducing elements of functional languages.
  - if式やswitch式のサポート
  - ブロック式による値のReturn
- Type system
  - 参照型のサポート
  - いずれかの型であることを表すユニオン型のサポート
- Generate LLVM IR code

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

# run
./debug/main
```

## Author
[@marihachi](https://github.com/marihachi)

## Contributors
- [@ikasoba](https://github.com/ikasoba) - [Details](https://github.com/marihachi/holo/issues?q=author%3Aikasoba)

## License
MIT License
