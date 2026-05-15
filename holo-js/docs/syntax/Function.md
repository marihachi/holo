## 関数定義
```
fn add(x: int, y: int): int {
  x + y
}
```

関数定義はLLVM-IRのdefine命令を生成する。
```llvm
define i32 @add(i32 %x, i32 %y) {
  ; ...
}
```

## 外部関数宣言
外部関数が存在することをコンパイラが認識するための宣言。  
オブジェクトコードをリンクする段階でその外部関数が存在すればコンパイルは成功する。  
```
external fn max(x: int, y: int): int;
```

この宣言はLLVM-IRのdeclare命令を生成する。
```llvm
declare i32 @max(i32 %x, i32 %y)
```
