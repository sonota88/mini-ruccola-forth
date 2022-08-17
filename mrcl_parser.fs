include lib/utils.fs
include lib/types.fs
include lib/json.fs

\ --------------------------------

create tokens-size_ 1 cells allot

: tokens-size! ( size -- )
    tokens-size_ !
;

\ --------------------------------

\ 「トークンのアドレスの配列」の開始アドレス
create tokens_ 10000 cells allot ( TODO 多めにしている )

create read-tokens-end_ 1 cells allot

: read-tokens-set-end ( end -- )
    read-tokens-end_ !
;

: read-tokens-end? ( pos -- bool )
    read-tokens-end_ @
    \ pos end
    >=
;

: read-tokens ( -- )
    read-stdin-all-v2
    \ src_ size

    str-dup
    \ src_ size | src_ size
    chars +
    \ src_ size | end

    read-tokens-set-end
    \ src_ size
    drop
    \ src_
    \ rest_
    0
    \ rest_ ti

    begin
        1 pick
        \ rest_ ti | rest_
        read-tokens-end? if
            \ rest_ ti
            dup
            \ rest_ ti | ti
            tokens-size!
            \ rest_ ti

            drop drop exit
            \ panic ( TODO )
        endif
        \ rest_ ti

        1 pick
        \ rest_ ti | rest_
        200 \ TODO dummy
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
        0
        10 \ LF
        \ rest_ ti | rest_ start-index char

        char-index
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
    again
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

: incr-pos ( -- )
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

: Token-kind-eq ( t_  kind_ size )
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
        \ ok
    else
        s" 208 unexpected token kind" type-e
        panic ( assertion failed )
    endif
;

: assert-val ( t_  val_ size -- )
    \ t_  val_ size
    Token-val-eq if
        \ ok
    else
        s" 218 unexpected token value" type-e
        panic ( assertion failed )
    endif
;

: consume ( kind_ size  val_ size -- )
    0 peek
    \ kind_ size  val_ size | t_
    4 pick
    4 pick
    \ kind_ size  val_ size | t_  kind_ size
    assert-kind
    \ kind_ size  val_ size

    0 peek
    \ kind_ size  val_ size | t_
    2 pick
    2 pick
    \ kind_ size  val_ size | t_  val_ size
    assert-val
    \ kind_ size  val_ size

    drop
    drop
    drop
    drop

    incr-pos
;

: consume-kw ( s_ size -- )
    s" kw"
    \ val_ size | kind_ size
    3 pick
    3 pick
    \ val_ size | kind_ size  kind_ size
    consume
    \ val_ size

    drop
    drop
;

: consume-sym ( s_ size -- )
    s" sym"
    \ val_ size | kind_ size
    3 pick
    3 pick
    \ val_ size | kind_ size  kind_ size
    consume
    \ val_ size

    drop
    drop
;

\ --------------------------------

: parse-return ( -- stmt_ )
    s" return" consume-kw

    0 peek
    \ t_
    s" ;" Token-val-eq if
        s" ;" consume-sym

        List-new
        \ stmt_
        s" return"
        \ stmt_  s_ size
        List-add-str-v2
        \ stmt_
    else
        List-new
        \ stmt_
        s" return"
        \ stmt_  s_ size
        List-add-str-v2
        \ stmt_

        0 peek incr-pos
        \ stmt_ t_
        Token-get-val
        \ stmt_ s_ size
        parse-int
        \ stmt_ n
        List-add-int-v2
        \ stmt_

        s" ;" consume-sym

        \ stmt_
    endif
;

: parse-stmt ( -- stmt_ )
    0 peek s" return"
    \ t_  val_ size
    Token-val-eq
    if
        \ (empty)
        parse-return
        \ return_

    else
        \ (empty)
        0 peek Json-print
        s" 324 failed to parse statement" type-e
        panic
    endif
;

: parse-func-def ( -- fn-def_ )
    List-new
    \ fn_

    \ ----------------

    s" func" List-add-str-v2
    \ fn_

    s" func" consume-kw

    \ ----------------
    \ fn name

    0 peek
    \ fn_ | t_
    Token-get-val
    \ fn_ | s_ size

    List-add-str-v2
    \ fn_

    incr-pos

    \ ----------------

    List-new
    \ fn_ fn-arg-names_
    List-add-list
    \ fn_

    s" (" consume-sym
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
                List-new
                \ fn_ stmts_ | stmt_

                s" var" consume-kw
                s" var" List-add-str-v2

                incr-pos \ a
                s" a" List-add-str-v2

                incr-pos \ ;

                \ fn_ stmts_ | stmt_
                List-add-list
                \ fn_ stmts_

            else
                parse-stmt
                \ fn_ stmts_ stmt_
                List-add-list
                \ fn_ stmts_

            endif

            false \ continue
        endif

    until

    \ fn_ stmts_
    List-add-list
    \ fn_

    s" }" consume-sym

    \ ----------------

    \ fn_
;

: parse-top-stmts ( -- top-stmts )
    List-new
    \ top_stmts_

    s" top_stmts" List-add-str-v2
    \ top_stmts_

    \ -->>
    parse-func-def
    \ top_stmts_ fn_
    List-add-list
    \ top_stmts_
    \ <<--

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
