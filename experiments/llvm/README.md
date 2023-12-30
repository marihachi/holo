Cからllvm-ir出力
```sh
clang -c -S -emit-llvm aaa.c bbb.c
```

llvm-irから実行ファイル生成
```sh
clang aaa.ll bbb.ll
```
