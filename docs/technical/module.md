### 仕様メモ
- プロジェクトのファイルリストに追加
- モジュールの公開メンバのフルネーム(名前空間+メンバ名)は重複不可、非公開メンバはモジュールを跨いだ識別子解決が必要ないため重複可


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
