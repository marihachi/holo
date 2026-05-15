target triple = "x86_64-unknown-linux-gnu"

define i32 @main() {
entry:
  ; int x;
  %x = alloca i32, align 4

  ; x = 1;
  store i32 1, ptr %x, align 4

  ; load x
  %0 = load i32, ptr %x, align 4

  ret i32 0
}
