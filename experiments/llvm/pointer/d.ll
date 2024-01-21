target triple = "x86_64-unknown-linux-gnu"

define void @d() {
entry:
  ; int *x;
  %x = alloca ptr, align 8

  ; x = (int *)1;
  store ptr inttoptr (i64 1 to ptr), ptr %x, align 8

  ret void
}
