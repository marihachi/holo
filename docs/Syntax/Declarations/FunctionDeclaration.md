## 関数宣言

宣言のみの場合:
```
fn add(x: int, y: int): int;
```
生成コード:
```c
int add(int x, int y);
```

関数本体を定義する場合:
```
fn add(x: int, y: int): int {
  return x + y;
}
```
生成コード:
```c
int add(int x, int y) {
  return x + y;
}
```

## 外部関数宣言
外部関数が存在することをコンパイラが認識するための宣言。  
オブジェクトコードをリンクする段階でその外部関数が存在すればコンパイルは成功する。  
```
extern fn max(x: int, y: int): int;
```
生成コード:
```c
extern int max(int x, int y);
```
