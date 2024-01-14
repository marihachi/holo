target triple = "x86_64-unknown-linux-gnu"

define i32 @main() {
entry:
  ret i32 0
}

; int *x, v;
; x = &v;
; *x = 1;
define void @a() {
entry:
  %x = alloca ptr, align 8
  %v = alloca i32, align 4

  ; x = &v;
  store ptr %v, ptr %x, align 8

  ; *x = 1;
  %0 = load ptr, ptr %x, align 8
  store i32 1, ptr %0, align 4

  ret void
}

; int *x, *v;
; x = v;
define void @b() {
entry:
  %x = alloca ptr, align 8
  %v = alloca ptr, align 8

  ; x = v;
  %0 = load ptr, ptr %v, align 8
  store ptr %0, ptr %x, align 8

  ret void
}

; int *x, v, r;
; x = &v;
; r = *x;
define void @c() {
entry:
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

; int *x;
; x = (int *)1;
define void @d() {
entry:
  %x = alloca ptr, align 8

  ; x = (int *)1;
  store ptr inttoptr (i64 1 to ptr), ptr %x, align 8

  ret void
}
