# switch expression
一致
```
var x: int =
  switch (c) {
    case 'a' {
      1
    }
    case 'b' {
      2
    }
    case 'c' {
      3
    }
    default {
      0
    }
  };
```
↓
```c
int x;
switch (c) {
  case 'a': {
    x = 1;
    break;
  }
  case 'b': {
    x = 2;
    break;
  }
  case 'c': {
    x = 3;
    break;
  }
  default: {
    x = 0;
    break;
  }
}
```

いずれかに一致
```
var x: int =
  switch (c) {
    case 1 || 2 {
      1
    }
    case 3 || 4 {
      2
    }
    default {
      0
    }
  };
```
↓
```c
int x;
switch (c) {
  case 1:
  case 2: {
    x = 1;
    break;
  }
  case 3:
  case 4: {
    x = 2;
    break;
  }
  default: {
    x = 0;
    break;
  }
}
```
