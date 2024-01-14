target triple = "x86_64-unknown-linux-gnu"

define i32 @main() {
entry:
  ; allocate stack
  %0 = alloca i32, align 4

  ; store 1 to %0 (stack)
  store i32 1, ptr %0, align 4

  ; load from %0 (stack) to %1
  %1 = load i32, ptr %0, align 4

  ret i32 0
}
