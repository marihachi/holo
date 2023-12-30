@.str0 = constant [7 x i8] c"hello\0A\00"

define i32 @main() {
entry:
  call i32 (ptr, ...) @printf(ptr @.str0)
  ret i32 0
}

declare i32 @printf(ptr, ...)
