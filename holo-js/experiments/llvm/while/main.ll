target triple = "x86_64-unknown-linux-gnu"

define i32 @main()  {
entry:
  %retval = alloca i32, align 4
  %a = alloca i32, align 4
  store i32 0, ptr %retval, align 4
  store i32 0, ptr %a, align 4
  br label %while.cond

while.cond:
  %0 = load i32, ptr %a, align 4
  %cmp = icmp eq i32 %0, 2
  br i1 %cmp, label %while.body, label %while.end

while.body:
  %1 = load i32, ptr %a, align 4
  %cmp1 = icmp eq i32 %1, 0
  br i1 %cmp1, label %if.then, label %if.else

if.then:
  br label %while.cond ; continue

if.else:
  br label %while.end ; break

while.end:
  ret i32 0
}
