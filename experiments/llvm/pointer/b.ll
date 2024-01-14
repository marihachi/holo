target triple = "x86_64-unknown-linux-gnu"

define void @b() {
entry:
  ; int *x, *v;
  %x = alloca ptr, align 8
  %v = alloca ptr, align 8

  ; x = v;
  %0 = load ptr, ptr %v, align 8
  store ptr %0, ptr %x, align 8

  ret void
}
