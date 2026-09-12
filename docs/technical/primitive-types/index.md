# 配列やポインタのネスト
参考: https://enakai00.hatenablog.com/entry/20110808/1312783316

int型へのポインタの配列
```
// Holo -> C
var x: int*[3]; -> int *x[3];
```

int型配列へのポインタ
```
// Holo -> C
var x: int[3]*; -> int (*x)[3];
```

# bool型
## Desugar
intになる
- true --> (int)1
- false --> (int)0
