target triple = "x86_64-unknown-linux-gnu"

define void @a() {
entry:
  ; int *x, v;
  %x = alloca ptr, align 8        ; int**
  %v = alloca i32, align 4        ; int*

  ; x = &v;
  store ptr %v, ptr %x, align 8   ; store &v &x 8

  ; *x = 1;
  %x.0 = load ptr, ptr %x, align 8  ; loadすることでポインタの先のポインタを取得
  store i32 1, ptr %x.0, align 4    ; store 1 &x.0 4

  ret void
}
