target triple = "x86_64-unknown-linux-gnu"

define i32 @main() {
entry:
  ret i32 0
}

define void @a() {
entry:
; int *x, v;
; x = &v;
; *x = 1;
  %x = alloca ptr, align 8
  %v = alloca i32, align 4
  store ptr %v, ptr %x, align 8
  %0 = load ptr, ptr %x, align 8
  store i32 1, ptr %0, align 4

  ret void
}

define void @b() {
entry:
; int *x, *v;
; x = v;
  %x = alloca ptr, align 8
  %v = alloca ptr, align 8
  %0 = load ptr, ptr %v, align 8
  store ptr %0, ptr %x, align 8

  ret void
}

define void @c() {
entry:
; int *x, v, r;
; x = &v;
; r = *x;
  %x = alloca ptr, align 8
  %v = alloca i32, align 4
  %r = alloca i32, align 4
  store ptr %v, ptr %x, align 8
  %0 = load ptr, ptr %x, align 8
  %1 = load i32, ptr %0, align 4
  store i32 %1, ptr %r, align 4

  ret void
}

define void @d() {
entry:
; int *x;
; x = (int *)1;
  %x = alloca ptr, align 8
  store ptr inttoptr (i64 1 to ptr), ptr %x, align 8

  ret void
}
