rm -rf obj
mkdir obj
rm -rf dist
mkdir dist

cp src/io_util.holo dist/io_util.holo

clang -c -O2 src/io_util.c -o obj/io_util.o

ar r dist/holo.a obj/io_util.o
