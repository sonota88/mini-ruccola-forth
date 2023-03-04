include lib/utils.fs
include lib/types.fs
include lib/json.fs

\ --------------------------------

create label-index_ 1 cells allot

: label-index@ ( -- li )
    label-index_ @
;

: label-index! ( i -- )
    label-index_ !
;

: label-index-init ( -- )
    0 label-index!
;

: incr-label-index ( -- )
    label-index@
    1 +
    label-index!
;

\ --------------------------------

: Names-index ( names_  s_ size -- index flag )
    2 pick
    \ names_  s_ size | names_
    List-len 0
    \ names_  s_ size | n 0
    ?do
        \ names_  s_ size
        str-dup
        \ names_  s_ size  s_ size
        4 pick i
        \ names_  s_ size  s_ size | names_ i
        List-get-str \ TODO node type のチェックをするとベター
        \ names_  s_ size  s_ size | si_ size
        \ names_  s_ size | s_ size  si_ size
        str-eq if
            \ names_  s_ size
            str-drop
            drop
            i true
            unloop
            exit
        else
            \ names_  s_ size
        endif
    loop
    \ names_  s_ size
    str-drop
    drop
    -1 false
;

: Names-include? ( names_  s_ size -- bool )
    Names-index
    \ index flag
    drop-1
    \ flag
;

\ --------------------------------

(
0: fn arg names
1: lvar names
)
: Context-new ( -- ctx_ )
    here
    \ ctx_

    2 cells allot
    \ ctx_

    List-new
    \ ctx_ | args_
    1 pick
    \ ctx_ | args_ ctx_[0]
    !
    \ ctx_

    List-new
    \ ctx_ | lvars_
    1 pick
    1 cells +
    \ ctx_ | lvars_ ctx_[1]
    !
    \ ctx_
;

: Context-set-fn-arg-names-1 ( ctx_ args_ -- ctx_ )
    1 pick
    \ ctx_ args_ ctx_
    !
    \ ctx_
;

: Context-add-lvar-0 ( ctx_  s_ size -- )
    2 pick
    \ ctx_  s_ size  ctx_
    1 cells +
    \ ctx_  s_ size  ctx_[1]
    @
    \ ctx_  s_ size  lvars_
    2 str-pick
    \ ctx_  s_ size | lvars_  s_ size
    List-add-str-0
    \ ctx_  s_ size

    str-drop
    drop
;

: Context-dump ( ctx_ -- )
    ." context:" cr

    \ TODO fn args

    dup 0 cells +
    \ ctx_ ctx_[0]
    @
    \ ctx_ args_
    ."   fn arg names: " Json-print-oneline cr
    \ ctx_

    dup 1 cells +
    \ ctx_ ctx_[1]
    @
    \ ctx_ lvars_
    ."   lvar names: " Json-print-oneline cr
    \ ctx_
    drop
;

: Context-fn-arg-name? ( ctx_  s_ size -- bool )
    \ ctx_  s_ size
    2 pick
    \ ctx_  s_ size | ctx_
    \ ctx_  s_ size | ctx_[0]
    @
    \ ctx_  s_ size | args_
    2 str-pick
    \ ctx_  s_ size | args_  s_ size
    Names-include?
    \ ctx_  s_ size | include?
    drop-1
    drop-1
    drop-1
    \ include?
;

: Context-lvar-name? ( ctx_  s_ size -- bool )
    \ ctx_  s_ size
    2 pick
    \ ctx_  s_ size | ctx_
    1 cells +
    \ ctx_  s_ size | ctx_[1]
    @
    \ ctx_  s_ size | lvars_
    2 str-pick
    \ ctx_  s_ size | lvars_  s_ size
    Names-include?
    \ ctx_  s_ size | include?
    drop-1
    drop-1
    drop-1
    \ include?
;

: Context-fn-arg-disp ( ctx_  s_ size -- disp )
    \ ctx_  s_ size
    2 pick
    \ ctx_  s_ size | ctx_
    0 cells +
    \ ctx_  s_ size | ctx_[0]
    @
    \ ctx_  s_ size | lvars_
    2 str-pick
    \ ctx_  s_ size | lvars_  s_ size
    Names-index
    if
        \ ctx_  s_ size | index
        drop-1
        drop-1
        drop-1
        \ index
        2 +
        \ disp
    else
        \ ctx_  s_ size | index
        ." 174" panic
    endif
;

: Context-lvar-disp ( ctx_  s_ size -- disp )
    \ ctx_  s_ size
    2 pick
    \ ctx_  s_ size | ctx_
    1 cells +
    \ ctx_  s_ size | ctx_[1]
    @
    \ ctx_  s_ size | lvars_
    2 str-pick
    \ ctx_  s_ size | lvars_  s_ size
    Names-index
    if
        \ ctx_  s_ size | index
        drop-1
        drop-1
        drop-1
        \ index
        -1 swap
        \ -1 index
        -
        \ disp
    else
        \ ctx_  s_ size | index
        ." 201" panic
    endif
;

\ --------------------------------

: asm-prologue
    ."   push bp" cr
    ."   cp sp bp" cr
;

: asm-epilogue
    ."   cp bp sp" cr
    ."   pop bp" cr
;

\ --------------------------------

: gen-expr-add ( -- )
    ."   pop reg_b" cr
    ."   pop reg_a" cr
    ."   add_ab" cr
;

: gen-expr-mult ( -- )
    ."   pop reg_b" cr
    ."   pop reg_a" cr
    ."   mult_ab" cr
;

: gen-expr-eq ( -- )
    incr-label-index
    label-index@
    \ li

    ."   pop reg_b" cr
    ."   pop reg_a" cr

    ."   compare" cr
    ."   jump_eq then_" dup print-int cr

    ."   cp 0 reg_a" cr \ neq => false
    ."   jump end_eq_" dup print-int cr

    ." label then_" dup print-int cr
    ."   cp 1 reg_a" cr \ eq => true

    ." label end_eq_" dup print-int cr

    drop
;

: gen-expr-neq ( -- )
    incr-label-index
    label-index@
    \ li

    ."   pop reg_b" cr
    ."   pop reg_a" cr

    ."   compare" cr
    ."   jump_eq then_" dup print-int cr

    ."   cp 1 reg_a" cr \ neq => true
    ."   jump end_neq_" dup print-int cr

    ." label then_" dup print-int cr
    ."   cp 0 reg_a" cr \ eq => false

    ." label end_neq_" dup print-int cr

    drop
;

: gen-expr-int ( ctx_ expr_ -- )
    drop-1
    \ expr_
    Node-get-int
    \ n
    ."   cp "
    print-int
    ."  reg_a" cr
    \ (empty)
;

: gen-expr-str ( ctx_ expr_ -- )
    Node-get-str
    \ ctx_  s_ size
    2 pick
    \ ctx_  s_ size | ctx_
    2 str-pick
    \ ctx_  s_ size | ctx_  s_ size
    Context-lvar-name?
    if
        \ ctx_  s_ size
        2 pick
        \ ctx_  s_ size | ctx_
        2 str-pick
        \ ctx_  s_ size | ctx_  s_ size
        Context-lvar-disp
        \ ctx_  s_ size | disp

        drop-1
        drop-1
        drop-1
        \ disp

        ."   cp [bp:" print-int ." ] reg_a" cr

        \ (empty)
    else
        \ ctx_  s_ size
        2 pick
        \ ctx_  s_ size | ctx_
        2 str-pick
        \ ctx_  s_ size | ctx_  s_ size
        Context-fn-arg-name?
    if
        \ ctx_  s_ size
        2 pick
        \ ctx_  s_ size | ctx_
        2 str-pick
        \ ctx_  s_ size | ctx_  s_ size
        Context-fn-arg-disp
        \ ctx_  s_ size | disp

        drop-1
        drop-1
        drop-1
        \ disp

        ."   cp [bp:" print-int ." ] reg_a" cr

        \ (empty)
    else
        ." 221" panic
    endif
    endif
;

defer gen-expr

: gen-expr-binop ( ctx_ expr_ -- )
    Node-get-list
    \ ctx_ list_

    ( lhs )
    dup 1 List-get
    \ ctx_ list_ | expr_
    2 pick swap
    \ ctx_ list_ | ctx_ expr_
    gen-expr
    \ ctx_ list_
    ."   push reg_a" cr

    ( rhs )
    dup 2 List-get
    \ ctx_ list_ | expr_
    2 pick swap
    \ ctx_ list_ | ctx_ expr_
    gen-expr
    \ ctx_ list_
    ."   push reg_a" cr
    \ ctx_ list_

    dup 0
    \ ctx_ list_ | list_ 0
    List-get-str
    str-dup
    \ ctx_ list_ s_ size | s_ size
    s" +" str-eq
    if
        \ ctx_ list_  s_ size
        gen-expr-add
        \ ctx_ list_  s_ size
        str-drop
        drop
        drop
        \ (empty)
    else
        \ ctx_ list_  s_ size
        str-dup
        \ ctx_ list_  s_ size | s_ size
        s" *" str-eq
    if
        \ ctx_ list_  s_ size
        gen-expr-mult
        \ ctx_ list_  s_ size
        str-drop
        drop
        drop
        \ (empty)
    else
        \ ctx_ list_  s_ size
        str-dup
        \ ctx_ list_  s_ size | s_ size
        s" ==" str-eq
    if
        \ ctx_ list_  s_ size
        gen-expr-eq
        \ ctx_ list_  s_ size
        str-drop
        drop
        drop
        \ (empty)
    else
        \ ctx_ list_  s_ size
        str-dup
        \ ctx_ list_  s_ size | s_ size
        s" !=" str-eq
    if
        \ ctx_ list_  s_ size
        gen-expr-neq
        \ ctx_ list_  s_ size
        str-drop
        drop
        drop
        \ (empty)
    else
        ." 46" panic
    endif
    endif
    endif
    endif
;

( gen-expr )
:noname ( ctx_ expr_ -- )
    dup
    \ ctx_ expr_ | expr_
    Node-get-type Node-type-int = if
        \ ctx_ expr_
        gen-expr-int
    else dup Node-get-type Node-type-str = if
        \ ctx_ expr_
        gen-expr-str
    else dup Node-get-type Node-type-list = if
        \ ctx_ expr_
        gen-expr-binop
    else
        \ expr_
        ." 33 unsupported expr type"
        panic
    endif
    endif
    endif
;
is gen-expr

\ (return)
\ (return {expr})
: gen-return ( ctx_ stmt_ -- )
    dup List-len 2 = if
        \ ctx_ stmt_
        1 List-get
        \ ctx_ node_
        gen-expr
        \ (empty)
    else
        drop
        \ (empty)
    endif

    asm-epilogue
    ."   ret" cr

    \ (empty)
;

: _gen-set ( ctx_ dest_ expr_ -- )
    \ ctx_ dest_ expr_

    2 pick
    \ ctx_ dest_ expr_ | ctx_
    1 pick
    \ ctx_ dest_ expr_ | ctx_ expr_
    gen-expr
    \ ctx_ dest_ expr_
    drop
    \ ctx_ dest_

    dup
    \ ctx_ dest_ | dest_
    Node-get-str
    \ ctx_ dest_ | s_ size
    3 pick
    \ ctx_ dest_ | s_ size | ctx_
    2 str-pick
    \ ctx_ dest_ | s_ size | ctx_  s_ size

    Context-lvar-name?
    \ ctx_ dest_ | s_ size | flag
    if
        \ ctx_ dest_ | s_ size
        3 pick
        \ ctx_ dest_ | s_ size | ctx_
        2 str-pick
        \ ctx_ dest_ | s_ size | ctx_  s_ size
        Context-lvar-disp
        \ ctx_ dest_ | s_ size | disp

        ."   cp reg_a [bp:" print-int ." ]" cr
        \ ctx_ dest_ | s_ size

        str-drop
        drop
        drop
    else
        ." 304" panic
    endif

    \ (empty)
;

\ (set {dest} {initial-value})
: gen-set ( ctx_ stmt_ -- )
    \ ctx_ stmt_
    1 pick
    \ ctx_ stmt_ | ctx_

    1 pick
    \ ctx_ stmt_ | ctx_ stmt_
    1 List-get ( => node_ )
    \ ctx_ stmt_ | ctx_ dest_

    2 pick
    \ ctx_ stmt_ | ctx_ dest_ stmt_
    2 List-get
    \ ctx_ stmt_ | ctx_ dest_ expr_
    _gen-set
    \ ctx_ stmt_
    drop drop
;

\ (_cmt {comment})
: gen-vm-comment ( s_ size -- )
    ."   _cmt "

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

\ (fn-name arg1 arg2 ... argN)
: _gen-funcall ( ctx_ funcall_ -- )
    \ ctx_ funcall_

    dup List-rest
    \ ctx_ funcall_ args_
    List-reverse
    \ ctx_ funcall_ args-r_
    dup List-len
    \ ctx_ funcall_ args-r_ num-args

    0
    \ ctx_ funcall_ args-r_ | num-args 0
    ?do
        \ ctx_ funcall_ args-r_
        dup i
        \ ctx_ funcall_ args-r_ | args-r_ i
        List-get
        \ ctx_ funcall_ args-r_ | node_
        3 pick swap
        \ ctx_ funcall_ args-r_ | ctx_ node_
        gen-expr
        \ ctx_ funcall_ args-r_
        ."   push reg_a" cr
        \ ctx_ funcall_ args-r_
    loop
    drop
    \ ctx_ funcall_

    dup 0 List-get-str
    \ ctx_ funcall_  fn_name_ size

    ."   _cmt call~~"
    str-dup type
    cr
    ."   call "
    str-dup type
    cr
    \ ctx_ funcall_  fn_name_ size

    str-drop
    \ ctx_ funcall_
    List-len 1 -
    \ ctx_ num-args
    
    ."   add_sp " print-int  cr

    \ ctx_
    drop
;

\ (call *{funcall})
: gen-call ( ctx_ stmt_ -- )
    \ ctx_ stmt_
    dup
    \ ctx_ stmt_ | stmt_
    List-rest
    \ ctx_ stmt_ | funcall_
    2 pick swap
    \ ctx_ stmt_ | ctx_ funcall_
    _gen-funcall
    \ ctx_ stmt_

    drop drop
;

\ (call_set {dest} {funcall})
: gen-call-set ( ctx_ stmt_ -- )
    \ ctx_ stmt_

    1 pick
    \ ctx_ stmt_ | ctx_
    1 pick
    \ ctx_ stmt_ | ctx_ stmt_
    2 List-get-list
    \ ctx_ stmt_ | ctx_ funcall_
    _gen-funcall
    \ ctx_ stmt_

    1 pick 1 pick
    \ ctx_ stmt_ | ctx_ stmt_
    1 List-get-str
    \ ctx_ stmt_ | ctx_  s_ size
    Context-lvar-name? if
        \ ctx_ stmt_
        1 pick 1 pick
        \ ctx_ stmt_ | ctx_ stmt_
        1 List-get-str
        \ ctx_ stmt_ | ctx_  s_ size
        Context-lvar-disp
        \ ctx_ stmt_ | disp
        ."   cp reg_a [bp:" print-int ." ]" cr
        \ ctx_ stmt_
        drop drop
    else
        ." 465" panic
    endif
;

defer gen-stmts

\ (while {expr} {stmts})
: gen-while ( ctx_ stmt_ -- )
    incr-label-index
    label-index@
    \ ctx_ stmt_ li

    ." label while_" dup print-int cr

    \ ctx_ stmt_ li
    2 pick
    \ ctx_ stmt_ li | ctx_
    2 pick
    \ ctx_ stmt_ li | ctx_ stmt_
    1 List-get
    \ ctx_ stmt_ li | ctx_ expr_
    gen-expr
    \ ctx_ stmt_ li

    ."   cp 0 reg_b" cr
    ."   compare" cr

    ."   jump_eq end_while_" dup print-int cr

    \ ctx_ stmt_ li
    2 pick
    \ ctx_ stmt_ li | ctx_
    2 pick
    \ ctx_ stmt_ li | ctx_ stmt_
    2 List-get-list
    \ ctx_ stmt_ li | ctx_ stmts_
    gen-stmts
    \ ctx_ stmt_ li

    ."   jump while_" dup print-int cr

    ." label end_while_" dup print-int cr

    drop
    drop drop
;

: gen-when-cond-expr ( ctx_ when_ -- )
    \ ctx_ when_
    0
    \ ctx_ when_ 0
    List-get
    \ ctx_ expr_
    gen-expr
    \ (empty)
;

\ (case *{when-clauses})
: gen-case ( ctx_ stmt_ -- )
    incr-label-index
    label-index@
    \ ctx_ stmt_ li

    1 pick
    List-len
    1
    \ ctx_ stmt_ li | size 1
    ?do
        \ ctx_ stmt_ li
        ."   # -->>" cr

        1 pick i
        \ ctx_ stmt_ li | stmt_ i
        List-get-list
        \ ctx_ stmt_ li | when_

        3 pick
        \ ctx_ stmt_ li | when_ | ctx_
        1 pick
        \ ctx_ stmt_ li | when_ | ctx_ when_
        gen-when-cond-expr
        \ ctx_ stmt_ li | when_

        ."   cp 0 reg_b" cr
        ."   compare" cr

        ."   jump_eq end_when_" 1 pick print-int ." _" i 1 - print-int cr

        \ ctx_ stmt_ li | when_
        3 pick
        1 pick
        \ ctx_ stmt_ li | when_ | ctx_ when_
        List-rest
        \ ctx_ stmt_ li | when_ | ctx_ stmts_
        gen-stmts
        \ ctx_ stmt_ li | when_
        drop
        \ ctx_ stmt_ li

        ."   jump end_case_" dup print-int cr

        ." label end_when_" dup print-int ." _" i 1 - print-int cr
        ."   # <<--" cr
        \ ctx_ stmt_ li
    loop

    ." label end_case_" dup print-int cr

    drop drop drop
;

: gen-stmt ( ctx_ stmt_ -- )
    \ s" gen-stmt" puts-fn

    dup
    \ ctx_ stmt_ stmt_
    0
    \ ctx_ stmt_ | stmt_ 0
    List-get-str
    \ ctx_ stmt_ | s_ size
    \              ^^^^^^^stmt_[0]

    str-dup
    \ ctx_ stmt_ | s_ size | s_ size
    s" return" str-eq
    if
        \ ctx_ stmt_ | s_ size
        str-drop
        \ ctx_ stmt_
        gen-return
        \ (empty)

    else
        str-dup
        s" set" str-eq
    if
        \ ctx_ stmt_ | s_ size
        str-drop
        \ ctx_ stmt_
        gen-set
        \ (empty)

    else
        str-dup
        s" call" str-eq
    if
        \ ctx_ stmt_ | s_ size
        str-drop
        \ ctx_ stmt_
        gen-call

    else
        str-dup
        s" call_set" str-eq
    if
        \ ctx_ stmt_ | s_ size
        str-drop
        \ ctx_ stmt_
        gen-call-set

    else
        str-dup
        s" while" str-eq
    if
        \ ctx_ stmt_ | s_ size
        str-drop
        \ ctx_ stmt_
        gen-while

    else
        str-dup
        s" case" str-eq
    if
        \ ctx_ stmt_ | s_ size
        str-drop
        \ ctx_ stmt_
        gen-case

    else
        str-dup
        s" _cmt" str-eq
    if
        \ ctx_ stmt_ | s_ size
        str-drop
        \ ctx_ stmt_
        drop-1
        \ stmt_
        1 List-get-str
        gen-vm-comment
        \ (empty)

    else
        ." 287" panic
    endif
    endif
    endif
    endif
    endif
    endif
    endif
;

:noname ( ctx_ stmts_ -- )
    \ s" gen-stmts" puts-fn

    dup List-len 0
    \ ctx_ stmts_ | size 0
    ?do
        \ ctx_ stmts_
        1 pick
        \ ctx_ stmts_ | ctx_
        1 pick
        \ ctx_ stmts_ | ctx_ stmts_
        i List-get-list
        \ ctx_ stmts_ | ctx_ stmt_
        gen-stmt
        \ ctx_ stmts_
    loop
    \ ctx_ stmts_
    drop
    drop
;
is gen-stmts

\ (var {name})
\ (var {name} {initial-value})
: gen-var ( ctx_ stmt_ -- )
    s" gen-var" puts-fn

    ."   sub_sp 1" cr

    dup List-len
    \ ctx_ stmt_ size
    3 = if
        \ ctx_ stmt_
        1 pick
        \ ctx_ stmt_ | ctx_
        1 pick
        \ ctx_ stmt_ | ctx_ stmt_
        1 List-get ( => node_ )
        \ ctx_ stmt_ | ctx_ dest_
        2 pick
        \ ctx_ stmt_ | ctx_ dest_ stmt_
        2 List-get
        \ ctx_ stmt_ | ctx_ dest_ expr_
        _gen-set
        \ ctx_ stmt_
    endif

    drop drop
;

\ (func fn-name args body)
: gen-func-def ( fn-def_ -- )
    s" gen-func-def" puts-fn

    Context-new
    \ fn-def_ ctx_
    1 pick
    \ fn-def_ ctx_ | fn-def_
    2 List-get-list
    \ fn-def_ ctx_ | args_
    \ fn-def_ | ctx_ args_
    Context-set-fn-arg-names-1
    \ fn-def_ | ctx_

    \ fn-def_ ctx_
    1 pick
    1
    \ fn-def_ ctx_ | fn-def_ 1
    List-get-str
    \ fn-def_ ctx_ | fn-name_ size

    ." label " type cr
    \ fn-def_ ctx_

    asm-prologue

    1 pick
    \ fn-def_ ctx_ | fn-def_
    3 List-get-list
    \ fn-def_ ctx_ | stmts_
    dup List-len 0
    \ fn-def_ ctx_ | stmts_ size 0
    ?do
        \ fn-def_ ctx_ | stmts_
        dup i
        \ fn-def_ ctx_ | stmts_ | stmts_ i
        List-get-list
        \ fn-def_ ctx_ | stmts_ | stmt_
        dup
        \ fn-def_ ctx_ | stmts_ stmt_ | stmt_
        0
        \ fn-def_ ctx_ | stmts_ stmt_ | stmt_ 0
        List-get-str
        \ fn-def_ ctx_ | stmts_ stmt_ | s_ size
        \                               ^^^^^^^stmt_[0]

        str-dup
        \ fn-def_ ctx_ | stmts_ stmt_ | s_ size | s_ size
        s" var" str-eq
        if
            \ fn-def_ ctx_ | stmts_ stmt_ | s_ size
            str-drop
            \ fn-def_ ctx_ | stmts_ | stmt_

            2 pick
            \ fn-def_ ctx_ | stmts_ | stmt_ | ctx_
            1 pick
            \ fn-def_ ctx_ | stmts_ | stmt_ | ctx_ stmt_
            1 List-get-str
            \ fn-def_ ctx_ | stmts_ | stmt_ | ctx_  s_ size
            \                                       ^^^^^^^ lvar-name
            Context-add-lvar-0
            \ fn-def_ ctx_ | stmts_ | stmt_

            2 pick swap
            \ fn-def_ ctx_ | stmts_ | ctx_ stmt_
            gen-var
            \ fn-def_ ctx_ | stmts_

        else
            \ fn-def_ ctx_ | stmts_ stmt_ | s_ size
            str-drop
            \ fn-def_ ctx_ | stmts_ | stmt_
            2 pick swap
            \ fn-def_ ctx_ | stmts_ | ctx_ stmt_
            gen-stmt
            \ fn-def_ ctx_ | stmts_
        endif
    loop

    \ fn-def_ ctx_ stmts_
    drop
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
    read-stdin-all
    \ json_ size

    Json-parse
    \ tree_

    label-index-init
    codegen
;

main
bye
