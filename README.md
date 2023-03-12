Forth（Gforth）でかんたんな自作言語のコンパイラを書いた  
https://qiita.com/sonota88/items/cdc6322d802844dc0953

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
  $ LANG=C wc -l *.fs lib/*.fs
 1072 mrcl_codegen.fs
  543 mrcl_lexer.fs
  942 mrcl_parser.fs
  378 lib/json.fs
  421 lib/types.fs
  382 lib/utils.fs
 3738 total

  $ cat *.fs lib/*.fs | grep -v '^ \+\\' | wc -l
2776
```
