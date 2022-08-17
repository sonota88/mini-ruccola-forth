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

\ (return)
\ (return {expr})
: gen-return ( stmt_ -- )
    dup List-len 2 = if
        ."   cp 42 reg_a" cr \ TODO
    endif

    asm-epilogue
    ."   ret" cr
    drop
;

\ (var)
: gen-var ( stmt_ -- )
    ."   sub_sp 1" cr
    drop
;

: str-dup ( s_ size -- s_ size  s_ size )
    1 pick
    1 pick
;

: str-drop ( s_ size -- )
    drop drop
;

\ (func fn-name args body)
: gen-func-def ( fn-def_ -- )
    dup
    1
    \ fn-def_ | fn-def_ 1
    List-get-str
    \ fn-def_ | fn-name_ size

    ." label " type cr
    \ fn-def_

    asm-prologue

    dup
    \ fn-def_ | fn-def_
    3 List-get-list
    \ fn-def_ | stmts_
    dup List-len 0
    \ fn-def_ | stmts_ size 0
    ?do
        \ fn-def_ stmts_
        dup i
        \ fn-def_ stmts_ | stmts_ i
        List-get-list
        \ fn-def_ stmts_ | stmt_
        dup
        \ fn-def_ stmts_ stmt_ | stmt_
        0
        \ fn-def_ stmts_ stmt_ | stmt_ 0
        List-get-str
        \ fn-def_ stmts_ stmt_ | s_ size
        \                        ^^^^^^^stmt_[0]

        str-dup
        \ fn-def_ stmts_ stmt_ | s_ size | s_ size
        s" var" str-eq
        if
            \ fn-def_ stmts_ stmt_ | s_ size
            str-drop
            \ fn-def_ stmts_ | stmt_
            gen-var
            \ fn-def_ stmts_
        else
            \ fn-def_ stmts_ stmt_ | s_ size
            str-dup
            \ fn-def_ stmts_ stmt_ | s_ size | s_ size
            s" return" str-eq
        if
            \ fn-def_ stmts_ stmt_ | s_ size
            str-drop
            \ fn-def_ stmts_ stmt_
            gen-return
            \ fn-def_ stmts_
        else
            panic
        endif
        endif
    loop

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

    Json-parse
    \ tree_

    codegen
;

main
bye
