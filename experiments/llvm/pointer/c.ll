target triple = "x86_64-unknown-linux-gnu"

define void @c() {
entry:
  ; int *x, v, r;
  %x = alloca ptr, align 8
  %v = alloca i32, align 4
  %r = alloca i32, align 4

  ; x = &v;
  store ptr %v, ptr %x, align 8

  ; load *x
  %0 = load ptr, ptr %x, align 8
  %1 = load i32, ptr %0, align 4

  ; r = *x;
  store i32 %1, ptr %r, align 4

  ret void
}
