# switch expression
基本形
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

複数のcase
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
