# It is currently being developed in C#. We plan to replace the current implementation.

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
external fn putn(x: int): void;

var y: int;

fn mul(x: int, c: int): int {
  return
    when (c == 1) x
    else x + mul(x, c - 1);
}

fn main(): int {
  y = mul(2, 20);
  putn(y);
  return 0;
}
```

## How to compile (Windows)
`clang` command is required (clang 16 or 17 recommended).

```sh
# install holo
install.bat

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
