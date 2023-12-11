# if expression

if式が値を返す場合はthenブロックelseブロックの両方が値を返していなければならない。
```
var x = if (a == 1) { 1 } else { 2 };
```

値を返さない場合はif式にセミコロンは無くてもよい。この場合ExpressionStatementとして処理される。
```
if (a == 1) return 1;

if (a == 1) {
  printf("a\n");
} else {
  printf("b\n");
}
```
