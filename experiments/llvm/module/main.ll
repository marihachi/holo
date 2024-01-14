target triple = "x86_64-unknown-linux-gnu"

@.str = private unnamed_addr constant [4 x i8] c"%d\0A\00", align 1

define i32 @main() #0 {
entry:
  ; x = add(1, 2);
  %x = alloca i32, align 4
  %call = call i32 @add(i32 noundef 1, i32 noundef 2)
  store i32 %call, ptr %x, align 4
  %0 = load i32, ptr %x, align 4

  ; printf("%d\n", x);
  %call1 = call i32 (ptr, ...) @printf(ptr noundef @.str, i32 noundef %0)

  ret i32 0
}

; int add(int x, int y);
declare i32 @add(i32 noundef, i32 noundef) #1

declare i32 @printf(ptr noundef, ...) #1
