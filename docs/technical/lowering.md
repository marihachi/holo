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
    case 4 {
      2
    }
    default {
      3
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
  case 4: {
    x = 2;
    break;
  }
  default: {
    x = 3;
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
副作用のある処理の評価順が変わると評価結果にも影響が及ぶ可能性があるため、関数コールも式の脱糖の対象とします。  

以下の構文はC言語レベルでは文として扱われるため、これらの構文が含まれる式はコード生成が始まるまでには脱糖が終わっていなければなりません：
- if式
- switch式
- ブロック式

## コンテナ
直下に文および式を含むことのできるノードをコンテナと呼びます。  
- FunctionDecl
- While
- Block

## アルゴリズム
式ノードが:  
- If
- Switch
- Block
- Call
のいずれかで、  
その式ノードがコンテナの直下にはなく、  
文ノード:  
- VariableDecl
- Assign
- ExpressionStatement
のいずれかの直下にもない時は、  
その式ノードを独立の代入文として切り出します。  
代入文は現在の文ノードのひとつ前の位置に挿入します。  
また、元々式ノードがあった位置には代入先の変数の名前を持つReferenceノードをセットします。  

追加仕様  
- 内側で見つかったコンテナから順に処理。

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
