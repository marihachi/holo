## ローカル変数の宣言・定義

- すべての変数宣言は現在のスコープの最初の部分で宣言されるようにコードが生成されます。 (要検討)

```
fn f(): void {
  var x: int;
  var y: int = 0;
}
```

生成コード(.c ファイル):
```c
void f(void)
{
  int x;
  int y = 0;
}
```

## グローバル変数の宣言・定義

```
var a: int;
var b: int = 0;
export var x: int;
export var y: int = 0;
```

生成コード(.h ファイル):
```c
extern int x;
extern int y;
```

生成コード(.c ファイル):
```c
static int a;
static int b = 0;
int x;
int y = 0;
```

## 外部変数宣言
FFI用機能。

```
declare int x;
```

外部変数が存在することをCコンパイラに認識させ、Cファイル内で変数を利用できるようにする。  
オブジェクトコードをリンクする段階でその外部変数が存在すればコンパイルは成功する。  

生成コード(.c ファイル):
```c
extern int x;
```
