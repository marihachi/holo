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
  var a: int = 1;

  return if (a == 1) {
    0
  } else {
    1
  };
}
```

## How to compile
`clang` command is required (clang 16 or 17 recommended).

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
