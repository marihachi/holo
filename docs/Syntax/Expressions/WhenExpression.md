## when式

- 条件式はbool値を期待します。
- when式を使うと、各when句の条件式の評価結果がtrueである場合に、後続の式が評価されます。  
  いずれのwhen句の評価結果もfalseであった場合は、else句に指定された式が評価されます。
- when式は、1つ以上のwhen句と1つのelse句が存在しなくてはなりません。

文法:
```abnf
whenExpr = 1*("when" "(" expression ")" expression) ["else" expression]
```

使用例:
```
var x = when (a == 1) 1 else 2;

var y =
  when (a > b) 1
  when (a < b) -1
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
