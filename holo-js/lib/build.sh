rm -rf obj
mkdir obj
rm -rf dist
mkdir dist

cp src/system.ho dist/system.ho

clang -c -O2 src/io.c -o obj/io.o

ar r dist/system.a obj/io.o
