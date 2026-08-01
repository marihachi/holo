### 単一モジュール

```
// math.holo

module holo.math;

export fn add(a: int, b: int): int {
  return a + b;
}

export fn sub(a: int, b: int): int {
  return a - b;
}
```

### ファイル分割 モジュール

```
// math/add.holo

partial module holo.math;

export fn add(a: int, b: int): int {
  return a + b;
}
```
```
// math/sub.holo

partial module holo.math;

export fn sub(a: int, b: int): int {
  return a - b;
}
```
