target triple = "x86_64-unknown-linux-gnu"

define void @a() {
entry:
  ; int *x, v;
  %x = alloca ptr, align 8
  %v = alloca i32, align 4

  ; x = &v;
  store ptr %v, ptr %x, align 8

  ; *x = 1;
  %0 = load ptr, ptr %x, align 8
  store i32 1, ptr %0, align 4

  ret void
}
