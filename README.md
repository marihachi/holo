# holo
A system programming language.  
The Holo compiler generates C file, and passes it to an external C compiler.

WIP!

## Syntax image
```
namespace app;

import { putn } from system.libc;
import * from app.your.lib;

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

## Usage (Windows)
System requirements:
- Binaries of holo compiler (Holoc).
- `clang` command (clang >= 16).

Place the Holoc binaries in a location of your choice, and add that location to the PATH environment variable.  

```sh
# compile
Holoc main.holo -o main.exe

# run
main.exe
```

Note: The first time you launch the Holo compiler, a configuration file (holo-config.yml) is created in the compiler's installation directory.

## Build holo compiler (Windows)
System requirements:
- .NET Core 8.0 (for building of the holo compiler)

Use Visual Studio or dotnet command.

## Concept
- Modern syntax introducing elements of functional languages.
- Type system
  - 参照型のサポート
  - いずれかの型であることを表すユニオン型のサポート
- Transpile to C code

## Author
[@marihachi](https://github.com/marihachi)

## Contributors
- [@ikasoba](https://github.com/ikasoba) - [Details](https://github.com/marihachi/holo/issues?q=author%3Aikasoba)

## License
MIT License
