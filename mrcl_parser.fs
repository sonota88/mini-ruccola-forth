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

    1 pick
    1 pick
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

: Token-get-str ( t_ -- s_ size )
    \ : List-get ( list_ n -- node_ )
    2
    \ t_ 2
    List-get
    \ node_
    Node-get-str
    \ s_ size
;

\ --------------------------------

: consume-kw ( s_ size -- )
    0 peek
    \ s_ size  t_

    dup Token-get-kind
    \ s_ size  t_ | kind_ size
    s" kw" str-eq if
        \ ok
    else
        panic ( assertion failed )
    endif


    Token-get-str
    \ s_ size  s_ size
    str-eq if
        \ ok
    else
        panic ( assertion failed )
    endif

    incr-pos
;

: consume-sym ( s_ size -- )
    0 peek
    \ s_ size  t_

    dup Token-get-kind
    \ s_ size  t_ | kind_ size
    s" sym" str-eq if
        \ ok
    else
        panic ( assertion failed )
    endif


    Token-get-str
    \ s_ size  s_ size
    str-eq if
        \ ok
    else
        panic ( assertion failed )
    endif

    incr-pos
;

\ --------------------------------

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
    Token-get-str
    \ fn_ | s_ size

    List-add-str-v2
    \ fn_

    incr-pos

    \ ----------------

    List-new
    \ fn_ []
    List-add-list
    \ fn_

    s" (" consume-sym
    s" )" consume-sym

    \ ----------------

    List-new
    \ fn_ []
    List-add-list
    \ fn_

    s" {" consume-sym
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
