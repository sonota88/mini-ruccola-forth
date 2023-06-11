Forth（Gforth）でかんたんな自作言語のコンパイラを書いた  
https://zenn.dev/sonota88/articles/f48ee696ad1de4

```
  $ gforth -v
gforth 0.7.3
```

```
git clone --recursive https://github.com/sonota88/mini-ruccola-forth.git
cd mini-ruccola-forth

./docker.sh build
./test.sh all
```

```
  $ LANG=C wc -l *.fs lib/{types,utils}.fs
 1072 mrcl_codegen.fs
  543 mrcl_lexer.fs
  942 mrcl_parser.fs
  421 lib/types.fs
  382 lib/utils.fs
 3360 total

  $ cat *.fs lib/{types,utils}.fs | grep -v '^ \+\\' | wc -l
2491
```
