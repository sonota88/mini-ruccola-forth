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

\ --------------------------------

: gen-expr ( expr_ -- )
    dup
    \ expr_ expr_
    Node-get-type Node-type-int = if
        \ expr_
        Node-get-int
        \ n
        ."   cp "
        print-int
        ."  reg_a" cr
        \ (empty)
    else
        panic \ TODO
    endif
;

\ (return)
\ (return {expr})
: gen-return ( stmt_ -- )
    dup List-len 2 = if
        \ stmt_
        1 List-get
        \ stmt_ node_
        gen-expr
        \ stmt_
    endif

    asm-epilogue
    ."   ret" cr

    \ stmt_
    drop
;

: _gen-set ( dest expr_ -- )
    drop-1 \ TODO
    \ expr_

    gen-expr
    \ (empty)

    ."   cp reg_a [bp:"
    -1 print-int \ TODO
    ." ]" cr
;

\ (set {name} {initial-value})
: gen-set ( stmt_ -- )
    0 \ TODO dummy
    \ stmt_ | dest
    1 pick
    \ stmt_ | dest stmt_
    2 List-get
    \ stmt_ | dest expr_
    _gen-set
    \ stmt_

    drop
;

\ (fn-name arg1 arg2 ... argN)
: _gen-funcall ( funcall_ -- )
    dup List-rest
    \ funcall_ args_
    List-reverse
    \ funcall_ args-r_
    dup List-len
    \ funcall_ args-r_ num-args
    0
    \ funcall_ args-r_ | num-args 0
    ?do
        \ funcall_ args-r_
        dup i
        \ funcall_ args-r_ | args-r_ i
        List-get
        \ funcall_ args-r_ | node_
        gen-expr
        \ funcall_ args-r_
        ."   push reg_a" cr
        \ funcall_ args-r_
    loop
    drop
    \ funcall_

    dup 0 List-get-str
    \ funcall_  fn_name_ size

    ."   _cmt call~~"
    str-dup type
    cr
    ."   call "
    str-dup type
    cr
    \ funcall_  fn_name_ size

    str-drop
    \ funcall_
    List-len 1 -
    \ num-args
    
    ."   add_sp " print-int  cr
;

\ (call *{funcall})
: gen-call ( stmt_ -- )
    dup
    \ stmt_ | stmt_
    List-rest
    \ stmt_ | funcall_
    _gen-funcall
    \ stmt_

    drop
;

\ (_cmt {comment})
: gen-vm-comment ( stmt_ -- )
    ."   _cmt "

    1 List-get-str
    \ s_ size

    0 ?do
        dup
        i chars +
        c@
        dup 32 = if \ ' '
            drop
            126 emit \ '~'
        else
            emit
        endif
    loop

    cr

    \ s_
    drop
    \ (empty)
;

: gen-stmt ( stmt_ -- )
    dup
    \ stmt_ stmt_
    0
    \ stmt_ | stmt_ 0
    List-get-str
    \ stmt_ | s_ size
    \         ^^^^^^^stmt_[0]

    str-dup
    \ stmt_ | s_ size | s_ size
    s" return" str-eq
    if
        \ stmt_ | s_ size
        str-drop
        \ stmt_
        gen-return
        \ (empty)

    else
        str-dup
        s" set" str-eq
    if
        \ stmt_ | s_ size
        str-drop
        \ stmt_
        gen-set
        \ (empty)

    else
        str-dup
        s" call" str-eq
    if
        str-drop
        gen-call

    else
        str-dup
        s" _cmt" str-eq
    if
        \ stmt_ | s_ size
        str-drop
        \ stmt_
        gen-vm-comment
        \ (empty)

    else
        panic
    endif
    endif
    endif
    endif
;

\ (var {name})
\ (var {name} {initial-value})
: gen-var ( stmt_ -- )
    ."   sub_sp 1" cr

    dup List-len
    \ stmt_ size
    3 = if
        \ stmt_
        0 \ TODO dummy
        \ stmt_ | dest
        1 pick
        \ stmt_ | dest stmt_
        2 List-get
        \ stmt_ | dest expr_
        _gen-set
        \ stmt_
    endif

    drop
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
            str-drop
            \ fn-def_ stmts_ stmt_
            gen-stmt
            \ fn-def_ stmts_
        endif
    loop

    \ fn-def_ stmts_
    drop
    drop
    \ (empty)

    asm-epilogue

    ."   ret" cr
;

: gen-top-stmts ( top-stmts_ -- )
    dup List-len
    \ tss_ size
    1 ?do \ 1 <= i < size
        dup i
        \ tss_ | tss_ i
        List-get-list
        \ tss_ | top-stmt_
        gen-func-def
        \ tss_
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
