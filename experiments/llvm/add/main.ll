target triple = "x86_64-unknown-linux-gnu"

define i32 @main () {
entry:
  ; return 1 + -1;
  %0 = add i32 1, -1
  ret i32 %0
}
