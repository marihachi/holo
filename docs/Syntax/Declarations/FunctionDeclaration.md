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
外部関数が存在することをコンパイラに認識させることができる。  
オブジェクトコードをリンクする段階でその外部関数が存在すればコンパイルは成功する。  

使用例:
```
declare fn max(x: int, y: int): int;
```

生成コード(.h ファイル):
```c
int max(int x, int y);
```
