## if文

- 条件式はbool値を期待します。

```
if (a == 1) return 1;

if (a == 1) {
  printf("a\n");
} else {
  printf("b\n");
}
```
生成コード:
```c
if (a == 1) return 1;

if (a == 1) {
  printf("a\n");
} else {
  printf("b\n");
}
```
