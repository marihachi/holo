target triple = "x86_64-unknown-linux-gnu"

define i32 @add(i32 noundef %x, i32 noundef %y) {
entry:
  ; load parameter x
  %x.addr = alloca i32, align 4
  store i32 %x, ptr %x.addr, align 4
  %0 = load i32, ptr %x.addr, align 4

  ; load parameter y
  %y.addr = alloca i32, align 4
  store i32 %y, ptr %y.addr, align 4
  %1 = load i32, ptr %y.addr, align 4

  ; return x + y;
  %add = add nsw i32 %0, %1
  ret i32 %add
}
