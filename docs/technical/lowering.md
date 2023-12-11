# ifの脱糖
```
var x = if (a) { 1 } else { 2 };
```
↓
```c
int x;
if (a) {
  x = 1;
} else {
  x = 2;
}
```

# switchの脱糖
```
var x =
  switch (a) {
    case 0 || 2 {
      0
    }
    case 1 || 3 {
      1
    }
    default {
      2
    }
  };
```
↓
```c
int x;
switch (a) {
  case 0:
  case 2: {
    x = 0;
    break;
  }
  case 1:
  case 3: {
    x = 1;
    break;
  }
  default: {
    x = 2;
    break;
  }
}
```

# blockの脱糖
```
var x = { var a = 1; a };
```
↓
```c
int x;
{
  int a = 1;
  x = a;
}
```

# 式の脱糖
式の途中に出てくるif式やswitch式などをC言語レベルのswitch文、if文などへ変換しやすいように変形します。  
そのために、式全体を複数の文に分解します。  
評価順を維持するためには、式全体の各部分を文として分解しなければなりません。

以下の構文はC言語レベルでは文として扱われるため、これらの構文が含まれる式はコード生成が始まるまでには脱糖が終わっていなければなりません：
- if式
- switch式
- ブロック式

## コンテナ
直下に文および式を含むことのできるノードをコンテナと呼びます。  
分解された式は、コンテナの直下にノードが生成されます。
- FunctionDecl
- While
- Block

## アルゴリズム
式が単純になるまで式ノードを分解します。  

単純な式の定義:  
- 式全体がReference、Literal、Binary、Unaryのいずれか単体または組み合わせで構成された式。
- 式ノードのルートがIf、Switch、Block、Callのいずれかで、他の式を含まないか単純な式のみを含んだ式。

内側で見つかったコンテナから順に処理。  
バイナリノードは左から右の順に処理。  

式の変形  
1. コンテナに変数(a)を追加します。
2. 対象の式(t)を先に評価するために、変数aへ式tを代入する代入文を追加します。
3. 元の式の中で式tがあった部分を変数aの参照ノードで置き換えます。

## 変形イメージ
```
fn f() {
  var x = {
    var c = {
      var a = 1 + 2;
      a
    } + {
      var b = 3 * 4;
      b
    } * 5;
    c
  };
}
```
↓ 式の脱糖
```
fn f() {
  var x = {
    var t0, t1, t2;
    t0 = {
      var a = 1 + 2;
      a
    };
    t1 = {
      var b = 3 * 4;
      b
    };
    t2 = t1 * 5;
    var c = t0 + t2;
    c
  };
}
```
↓ blockの脱糖
```
fn f() {
  var x;
  {
    var t0, t1, t2;
    {
      var a = 1 + 2;
      t0 = a;
    };
    {
      var b = 3 * 4;
      t1 = b;
    };
    t2 = t1 * 5;
    var c = t0 + t2;
    x = c;
  };
}
```
↓ Cコード生成
```c
void f() {
  int x;
  {
    int t0, t1, t2;
    {
      int a = 1 + 2;
      t0 = a;
    }
    {
      int b = 3 * 4;
      t1 = b;
    }
    t2 = t1 * 5;
    int c = t0 + t2;
    x = c;
  }
}
```
