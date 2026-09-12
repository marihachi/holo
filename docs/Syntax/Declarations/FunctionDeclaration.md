## 関数定義

```
fn add(x: int, y: int): int {
  return x + y;
}
```

生成コード(.h ファイル):
```c
int add(int x, int y);
```

生成コード(.c ファイル):
```c
int add(int x, int y) {
  return x + y;
}
```

## 外部関数宣言
FFI用機能。

使用例:
```
declare fn max(x: int, y: int): int;
```

外部関数が存在することをCコンパイラに認識させ、Cファイル内で関数を利用できるようにする。  
オブジェクトコードをリンクする段階でその外部関数が存在すればコンパイルは成功する。  

生成コード(.c ファイル):
```c
int max(int x, int y);
```
