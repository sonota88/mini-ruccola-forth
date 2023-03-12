include lib/utils.fs
include lib/types.fs
include lib/json.fs

\ --------------------------------

\ トークンの配列の要素の先頭アドレス
create tokens-pos_ 1 cells allot

: tokens-pos! ( size -- )
    tokens-pos_ !
;

: tokens-pos@ ( -- size )
    tokens-pos_ @
;

\ --------------------------------

\ トークンの数
create tokens-size_ 1 cells allot

: tokens-size! ( size -- )
    tokens-size_ !
;

: tokens-size@ ( -- size )
    tokens-size_ @
;

: tokens-size-incr ( -- )
    tokens-size@
    1 +
    tokens-size!
;

: tokens-size-init ( -- )
    0 tokens-size!
;

\ --------------------------------

\ 「トークンのアドレスの配列」の開始アドレス
create tokens_ 10000 cells allot \ 多めに確保

create read-tokens-end_ 1 cells allot

: read-tokens-set-end ( end_ -- )
    read-tokens-end_ !
;

: read-tokens-end? ( pos -- bool )
    read-tokens-end_ @
    \ pos end
    >=
;

: read-tokens ( -- )
    read-stdin-all
    \ src_ size

    str-dup
    \ src_ size | src_ size
    chars +
    \ src_ size | end_

    read-tokens-set-end
    \ src_ size
    drop
    \ src_
    \ rest_
    0
    \ rest_ ti

    tokens-size-init

    begin
        1 pick
        \ rest_ ti | rest_
        read-tokens-end? if
            \ rest_ ti
            dup
            \ rest_ ti | ti
            tokens-pos!
            \ rest_ ti

            drop drop exit
        endif
        \ rest_ ti

        1 pick
        \ rest_ ti | rest_
        200 \ dummy size
        \ rest_ ti | rest_ size
        Json-parse
        \ rest_ ti | t_

        tokens_
        \ rest_ ti t_ | ts_
        2 pick
        \ rest_ ti t_ | ts_ ti
        +
        \ rest_ ti t_ | ts_+ti

        \ rest_ ti | t_ ts_+ti
        ! ( set token )
        \ rest_ ti

        1 pick
        200 \ dummy size
        \ rest_ ti | rest_ size

        lf-index
        \ rest_ ti | index flag
        check-and-panic
        \ rest_ ti | index

        dup 0 <= if
            \ rest_ ti
            panic ( must not happen )
        endif
        \ rest_ ti index

        2 pick
        \ rest_ ti | index rest_
        swap
        \ rest_ ti | rest_ index
        chars +
        1 chars + ( consume LF )
        \ rest_ ti rest_next_

        1 pick
        \ rest_ ti rest_next_ | ti
        1 cells +
        \ rest_ ti rest_next_ | ti_next

        drop-2
        drop-2
        \ rest_next_ ti_next

        tokens-size-incr

        \ rest_next_ ti_next
    again

    panic \ must not happen
;

\ --------------------------------

create pos_ 1 cells allot

: pos@ ( -- pos )
    pos_ @
;

: pos! ( pos -- )
    pos_ !
;

: pos-init ( -- )
    0 pos!
;

: pos++ ( -- )
    pos@
    \ pos
    1 +
    \ pos+1
    pos!
    \ (empty)
;

: peek ( offset -- t_ )
    tokens_
    \ offset | ts_
    pos@
    \ offset | ts_ pos
    cells +
    \ offset | ts_+pos

    swap
    \ ts_+pos offset
    cells +
    \ ts_+pos+offset

    @
    \ t_
;

\ --------------------------------

: Token-get-kind ( t_ -- kind_ size )
    \ : List-get ( list_ n -- node_ )
    1
    \ t_ 1
    List-get
    \ node_
    Node-get-str
    \ s_ size
;

: Token-get-val ( t_ -- s_ size )
    \ : List-get ( list_ n -- node_ )
    2
    \ t_ 2
    List-get
    \ node_
    Node-get-str
    \ s_ size
;

: Token-get-intval ( t_ -- n )
    Token-get-val
    parse-int
;

: Token-kind-eq ( t_  kind_ size -- flag )
    2 pick
    \ t_ | kind_ size  t_
    Token-get-kind
    \ t_ | kind_ size  kind_ size
    str-eq
    \ t_ | bool

    drop-1
    \ bool
;

: Token-val-eq ( t_  val_ size )
    2 pick
    \ t_ | val_ size  t_
    Token-get-val
    \ t_ | val_ size  val_ size
    str-eq
    \ t_ | bool

    drop-1
    \ bool
;

\ --------------------------------

: assert-kind ( t_  kind_ size -- )
    \ t_  kind_ size
    Token-kind-eq if
        ( ok )
    else
        s" unexpected token kind" type-e
        panic ( assertion failed )
    endif
;

: assert-val ( t_  val_ size -- )
    \ t_  val_ size
    2 pick
    \ t_  val_ size | t_
    2 str-pick
    \ t_  val_ size | t_  val_ size

    Token-val-eq if
        \ t_  val_ size
        str-drop
        drop
        ( ok )
    else
        \ t_  val_ size
        ." expected("
        \ t_  val_ size
        type
        ." )" cr

        Token-get-val
        ." actual("
        \ val_ size
        type
        ." )" cr

        ." unexpected token value"
        panic ( assertion failed )
    endif
;

: consume ( kind_ size  val_ size -- )
    0 peek
    \ kind_ size  val_ size | t_
    4 str-pick
    \ kind_ size  val_ size | t_  kind_ size
    assert-kind
    \ kind_ size  val_ size

    0 peek
    \ kind_ size  val_ size | t_
    2 str-pick
    \ kind_ size  val_ size | t_  val_ size
    assert-val
    \ kind_ size  val_ size

    str-drop
    str-drop
    \ (empty)

    pos++
;

: consume-kw ( s_ size -- )
    s" kw"
    \ val_ size | kind_ size
    3 str-pick
    \ val_ size | kind_ size  kind_ size
    consume
    \ val_ size

    str-drop
    \ (empty)
;

: consume-sym ( s_ size -- )
    s" sym"
    3 str-pick
    consume
    str-drop
;

\ --------------------------------

: parse-arg ( -- arg_ )
    0 peek
    \ t_
    dup s" ident" Token-kind-eq if
        \ t_
        Token-get-val
        \ s_ size
        Node-new-str
        \ node_
    else dup s" int" Token-kind-eq if
        \ t_
        Token-get-intval
        \ n
        Node-new-int
        \ node_
    else
        ." unsupported"
        panic
    endif
    endif
;

: parse-args ( -- args_ )
    List-new
    \ args_

    0 peek
    s" )" Token-val-eq if
        \ args_
        exit
    endif

    parse-arg
    \ args_ node_
    List-add-1
    \ args_
    pos++

    begin
        0 peek
        \ args_ t_
        s" ," Token-val-eq if
            \ args_
            s" ," consume-sym

            parse-arg
            \ args_ node_
            List-add-1
            \ args_
            pos++
            
            false \ continue
        else
            \ args_
            true \ break
        endif
    until
;

: make-binop-expr ( lhs_  op_ size  rhs_ -- expr_ )
    \ lhs_  op_ size  rhs_
    List-new
    \ lhs_  op_ size  rhs_ | list_

    3 str-pick
    List-add-str-1
    \ lhs_  op_ size  rhs_ | list_

    4 pick List-add-1
    \ lhs_  op_ size  rhs_ | list_
    1 pick List-add-1
    \ lhs_  op_ size  rhs_ | list_
    Node-new-list
    \ op_ size  lhs_ rhs_ | node_
    drop-1
    drop-1
    drop-1
    drop-1
    \ node_
;

defer parse-expr

: parse-expr-factor ( -- expr_node_ )
    s" parse-expr-factor" puts-fn

    0 peek
    \ t_
    dup s" int" Token-kind-eq if
        \ t_
        pos++
        Token-get-intval
        \ n
        Node-new-int
        \ node_
    else dup s" ident" Token-kind-eq if
        \ t_
        pos++
        Token-get-val
        \ s_ size
        Node-new-str
        \ node_
    else dup s" sym" Token-kind-eq if
        \ t_
        drop
        s" (" consume-sym
        parse-expr
        \ node_
        s" )" consume-sym
        \ node_
    else
        ." unexpected token kind"
        panic
    endif
    endif
    endif
;

: binop? ( t_ -- bool )
    Token-get-val
    \ s_ size
    str-dup s" +" str-eq if
        \ s_ size
        str-drop
        true
    else
        \ s_ size
        str-dup s" *" str-eq
    if
        str-drop true

    else str-dup s" ==" str-eq if
        str-drop true

    else str-dup s" !=" str-eq if
        str-drop true

    else
        \ s_ size
        str-drop
        false
    endif
    endif
    endif
    endif
;

( parse-expr )
:noname  ( -- expr_node_ )
    s" parse-expr" puts-fn

    parse-expr-factor
    \ expr_

    begin
        0 peek binop? if
            \ expr_
            0 peek Token-get-val
            \ expr_ | s_ size
            pos++

            parse-expr-factor
            \ expr_  s_ size  rhs_

            make-binop-expr
            \ new_expr_

            false
        else
            true
        endif
    until

    \ node_
;
is parse-expr

: parse-return ( -- stmt_ )
    s" return" consume-kw

    List-new
    \ stmt_
    s" return"
    \ stmt_  s_ size
    List-add-str-1
    \ stmt_

    0 peek
    \ stmt_ t_
    s" ;" Token-val-eq if
        ( pass )
    else
        \ stmt_
        parse-expr
        \ stmt_ expr_
        List-add-1
        \ stmt_
    endif

    s" ;" consume-sym
    \ stmt_
;

: parse-set ( -- stmt_ )
    s" parse-set" puts-fn

    List-new
    \ stmt_

    s" set" consume-kw
    s" set" List-add-str-1

    0 peek
    \ stmt_ | t_
    Token-get-val
    \ stmt_ | s_ size
    List-add-str-1
    \ stmt_
    pos++
    \ stmt_

    s" =" consume-sym

    parse-expr
    \ stmt_ expr_
    List-add-1
    \ stmt_

    s" ;" consume-sym
;

: parse-call ( -- stmt_ )
    List-new
    \ stmt_

    s" call" consume-kw
    s" call" List-add-str-1

    0 peek pos++
    Token-get-val
    \ stmt_  fn-name_ size
    List-add-str-1
    \ stmt_

    s" (" consume-sym
    parse-args
    \ stmt_ args_
    List-add-all-1
    \ stmt_
    s" )" consume-sym

    s" ;" consume-sym

    \ stmt_
;

: parse-call-set ( -- stmt_ )
    s" parse-call-set" puts-fn

    List-new
    \ stmt_
    s" call_set" List-add-str-1
    \ stmt_

    s" call_set" consume-kw
    \ stmt_ funcall_

    ( var name )
    0 peek
    \ stmt_ funcall_ t_
    Token-get-val
    List-add-str-1
    \ stmt_ funcall_
    pos++

    s" =" consume-sym

    List-new
    \ stmt_ funcall_

    0 peek
    Token-get-val
    \ stmt_ funcall_  fname_ size
    List-add-str-1
    \ stmt_ funcall_
    pos++

    s" (" consume-sym
    parse-args
    \ stmt_ | funcall_ args_
    List-add-all-1
    \ stmt_ | funcall_
    
    s" )" consume-sym
    s" ;" consume-sym

    \ stmt_ funcall_
    List-add-list-1
    \ stmt_
;

defer parse-stmts

: parse-while ( -- stmt_ )
    s" parse-while" puts-fn

    List-new
    \ stmt_
    s" while" List-add-str-1

    s" while" consume-kw
    s" (" consume-sym

    parse-expr
    \ stmt_ expr_
    List-add-1

    s" )" consume-sym

    s" {" consume-sym

    parse-stmts
    \ stmt_ stmts_
    List-add-list-1
    \ stmt_

    s" }" consume-sym
    \ stmt_
;

: _parse-case-when-clause ( -- when-clause_ )
    List-new
    \ when_

    s" when" consume-kw

    s" (" consume-sym
    parse-expr
    \ when_ cond_
    List-add-1
    \ when_
    s" )" consume-sym

    s" {" consume-sym

    parse-stmts
    \ when_ stmts_
    List-add-all-1
    \ when_

    s" }" consume-sym

    \ when_
;

: parse-case ( -- stmt_ )
    s" case" consume-kw

    List-new
    \ stmt_
    s" case" List-add-str-1
    \ stmt_

    begin
        0 peek
        \ stmt_ t_
        s" when" Token-val-eq if
            \ stmt_
            _parse-case-when-clause
            \ stmt_ when_

            List-add-list-1
            \ stmt_
            false
        else
            \ stmt_
            true
        endif
    until
;

: parse-vm-comment ( -- stmt_ )
    s" _cmt" consume-kw
    s" (" consume-sym

    0 peek pos++
    \ t_
    Token-get-val
    \ s_ size

    s" )" consume-sym
    s" ;" consume-sym

    \ s_ size
    List-new
    \ s_ size  stmt_
    s" _cmt" List-add-str-1
    2 str-pick
    \ s_ size  stmt_ | s_ size
    List-add-str-1
    \ s_ size  stmt_
    drop-1
    drop-1
    \ stmt_
;

: parse-stmt ( -- stmt_ )
    s" parse-stmt" puts-fn

    0 peek s" return"
    \ t_  val_ size
    Token-val-eq
    if
        \ (empty)
        parse-return
        \ stmt_
    else 0 peek s" set"      Token-val-eq if parse-set
    else 0 peek s" call"     Token-val-eq if parse-call
    else 0 peek s" call_set" Token-val-eq if parse-call-set
    else 0 peek s" while"    Token-val-eq if parse-while
    else 0 peek s" case"     Token-val-eq if parse-case
    else 0 peek s" _cmt"     Token-val-eq if parse-vm-comment
    else
        \ (empty)
        ." failed to parse statement" cr
        0 peek Json-print
        panic
    endif
    endif
    endif
    endif
    endif
    endif
    endif

    \ stmt_
;

:noname ( -- stmts_ )
    s" parse-stmts" puts-fn

    List-new
    \ stmts_

    begin
        \ stmts_
        0 peek
        s" }" Token-val-eq if
            \ stmts_
            true \ break
        else
            \ stmts_
            parse-stmt
            \ stmts_ stmt_
            List-add-list-1
            \ stmts_
            false \ continue
        endif
    until

    \ stmts_
;
is parse-stmts

: parse-var ( -- stmt_ )
    s" parse-var" puts-fn

    List-new
    \ stmt_

    s" var" consume-kw
    s" var" List-add-str-1

    0 peek
    \ stmt_ | t_
    Token-get-val
    \ stmt_ | s_ size
    List-add-str-1
    \ stmt_
    pos++
    \ stmt_

    0 peek
    \ stmt_ | t_
    s" =" Token-val-eq if
        s" =" consume-sym

        parse-expr
        \ stmt_ expr_
        List-add-1
        \ stmt_
    endif

    s" ;" consume-sym
;

: parse-func-def ( -- fn-def_ )
    s" parse-func-def" puts-fn
    List-new
    \ fn_

    \ ----------------

    s" func" List-add-str-1
    \ fn_

    s" func" consume-kw

    \ ----------------
    \ fn name

    0 peek
    \ fn_ | t_
    Token-get-val
    \ fn_ | s_ size

    List-add-str-1
    \ fn_

    pos++

    \ ----------------

    s" (" consume-sym

    parse-args
    \ fn_ fn-arg-names_
    List-add-list-1
    \ fn_

    s" )" consume-sym

    \ ----------------

    s" {" consume-sym

    \ fn_
    List-new
    \ fn_ stmts_

    begin

        0 peek
        \ fn_ stmts_ t_
        dup s" }" Token-val-eq if
            \ fn_ stmts_ t_
            drop
            \ fn_ stmts_

            true \ break / end of statements
        else
            \ fn_ stmts_ t_

            s" var" Token-val-eq if
                \ fn_ stmts_
                parse-var
                \ fn_ stmts_ | stmt_
                List-add-list-1
                \ fn_ stmts_

            else
                parse-stmt
                \ fn_ stmts_ stmt_
                List-add-list-1
                \ fn_ stmts_

            endif

            false \ continue
        endif

    until

    \ fn_ stmts_
    List-add-list-1
    \ fn_

    s" }" consume-sym

    \ ----------------

    \ fn_
;

: parse-top-stmts ( -- top-stmts )
    List-new
    \ top_stmts_

    s" top_stmts" List-add-str-1
    \ top_stmts_

    begin
        \ top_stmts_
        parse-func-def
        \ top_stmts_ fn_
        List-add-list-1
        \ top_stmts_

        tokens-size@ pos@ <=
    until

    \ top_stmts_
;

: main
    read-tokens

    pos-init
    parse-top-stmts
    \ top_stmts_
    
    Json-print
;

main
bye
