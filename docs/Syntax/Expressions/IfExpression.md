## if式

- if式はif句とelse句で構成されます。
- 条件式はbool値を期待します。
- if句の条件式の評価結果がtrueである場合に、後続の式(then式と呼びます)が評価されます。  
  前述の評価結果がfalseの場合は、else句の式(else式と呼びます)が評価されます。

文法:
```abnf
ifExpr = "if" "(" expression ")" expression "else" expression
```

使用例:
```
var x = if (a == 1) 1 else 2;

var y =
  if (a > b) 1
  else if (a < b) -1
  else 0;
```

生成コード:
```c
int x = (a == 1) ? 1 : 2;

int y =
  (a > b) ? 1 :
  (a < b) ? -1 :
  0;
```
