# holo
A system programming language of modern syntaxes that transpiles to C.

WIP!

## Concept
- Modern syntax introducing elements of functional languages.
- Type system
  - 参照型のサポート
  - いずれかの型であることを表すユニオン型のサポート
- Transpile to C code

## Syntax image
```
var x: int;

fn main(argc: int, argv: char*[]): int {
  var x: int = 1;

  return when (x == 1) 0 else 1;
}
```

## How to compile (Windows)
`clang` command is required (clang 16 or 17 recommended).

```sh
# install holo
install.sh

# compile
Holoc.exe main.holo lib/dist/system.a -o main.exe

# run
main.exe
```

## Author
[@marihachi](https://github.com/marihachi)

## Contributors
- [@ikasoba](https://github.com/ikasoba) - [Details](https://github.com/marihachi/holo/issues?q=author%3Aikasoba)

## License
MIT License
