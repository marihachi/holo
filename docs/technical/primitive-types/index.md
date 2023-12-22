# 配列やポインタのネスト
参考: https://enakai00.hatenablog.com/entry/20110808/1312783316

```
var x1: int[3];
var x2: int*[3];
var x3: int[3]*;
```
```
int x1[3];
int *x2[3];
int (*x3)[3];
```

# bool型
## Desugar
intになる
- true --> (int)1
- false --> (int)0
