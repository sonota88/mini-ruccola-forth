include lib/utils.fs
include lib/types.fs
include lib/json.fs

: asm-prologue
    ."   push bp" cr
    ."   cp sp bp" cr
;

: asm-epilogue
    ."   cp bp sp" cr
    ."   pop bp" cr
;

: gen-func-def ( fn-def_ -- )
    dup
    1
    \ fn-def_ | fn-def_ 1
    List-get-str
    \ fn-def_ | fn-name_ size

    ." label " type cr

    asm-prologue

    asm-epilogue

    ."   ret" cr
;

: gen-top-stmts ( top-stmts_ -- )
    dup List-len
    \ tss_ size
    1 do \ 1 <= i < size
        dup i
        \ tss_ | tss_ i
        List-get-list
        \ tss_ | top-stmt_
        gen-func-def
    loop
;

: codegen ( tree_ -- )
    ."   call main" cr
    ."   exit" cr

    \ tree_
    gen-top-stmts
;

: main
    read-stdin-all-v2
    \ json_ size
    drop
    \ json_

    Json-parse
    \ tree_

    codegen
;

main
bye
