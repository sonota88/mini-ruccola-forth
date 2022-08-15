include lib/utils.fs
include lib/types.fs
include lib/json.fs

\ --------------------------------

create src-end_ 1 cells allot

: set-src-end ( pos -- )
    src-end_ !
    ( empty )
;

: end? ( pos -- )
    src-end_ @
    \ pos src_end
    >=
    ( is-end )
;

\ --------------------------------

: alphabet? ( c -- bool )
    dup 97 < if \ 'a'
        drop false exit
    endif

    dup 122 > if \ 'z'
        drop false exit
    endif

    drop true
;

: ident-char? ( c -- bool )
    dup alphabet? if
        drop true exit
    endif

    \ TODO large letter

    dup 95 = if \ '_'
        drop true exit
    endif

    drop false
;

: non-ident-index ( s_ size -- index ok )
    1 pick
    \ s_beg_ size s_
    begin
        \ s_beg_ size s_

        dup
        \ s_beg_ size s_ | s_
        3 pick
        \ s_beg_ size s_ | s_ s_beg_
        -
        \ s_beg_ size s_ | delta
        2 pick
        \ s_beg_ size s_ | delta size
        >= if
            \ s_beg_ size s_
            drop drop drop
            -1 false
            exit
        endif

        \ s_beg_ size s_
        dup c@
        \ s_beg_ size s_ c

        ident-char? \ s_beg_ size s_ | bool
        if
            \ (continue)
        else
            \ s_beg_ size s_
            2 pick
            \ s_beg_ size s_ s_beg_ 
            -
            \ s_beg_ size index
            drop-1
            drop-1
            true
            exit
        endif

        \ s_beg_ size s_
        1 chars +
    again

    panic
;

: match-ident ( rest_ -- num-chars )
    100 \ TODO dummy
    \ rest_ size
    non-ident-index
    \ index ok
;

: start-with-func? ( rest_ -- bool )
    4
    \ rest_ 4
    s" func"

    compare \ => 等しい場合 0
    0 =
;

: symbol? ( c -- bool )
    dup 40 = if \ '('
        drop
        true exit
    endif

    dup 41 = if \ ')'
        drop
        true exit
    endif

    dup 123 = if \ '{'
        drop
        true exit
    endif

    dup 125 = if \ '}'
        drop
        true exit
    endif

    drop
    false
;

: print-token ( lineno kind_ size s_ size -- )
    List-new
    \ lineno kind_ size s_ size list_

    5 pick
    \ lineno kind_ size s_ size | list_ lineno
    List-add-int-v2
    \ lineno kind_ size s_ size | list_

    4 pick
    4 pick
    \ lineno kind_ size s_ size | list_  kind_ size
    List-add-str-v2
    \ lineno kind_ size s_ size | list_

    2 pick
    2 pick
    \ lineno kind_ size s_ size | list_  s_ size
    List-add-str-v2
    \ lineno kind_ size s_ size | list_

    Json-print
    \ lineno kind_ size s_ size

    drop
    drop
    drop
    drop
    drop
;

: print-func-token ( -- )
    1 s" kw" s" func" print-token
;

: print-ident-token ( rest_ size -- )
    1 s" ident"
    \ rest_ size | 1  kind_ size
    4 pick
    4 pick
    \ rest_ size | 1  kind_ size  rest_ size
    print-token
    \ rest_ size

    drop drop
;

: print-sym-token ( s_ size -- )
    1 s" sym"
    \ s_ size | 1 kind_ size
    4 pick
    \ s_ size | 1 kind_ size s_
    4 pick
    \ s_ size | 1 kind_ size s_ size
    print-token
    \ s_ size

    drop drop
;

: char-to-s ( c -- s_ size )
    s" X"
    \ c s_ size
    2 pick
    \ c s_ size | c
    2 pick
    \ c s_ size | c s_
    ! ( set char )
    \ c s_ size

    drop-2
    \ s_ size
;

: main
    read-stdin-all-v2
    \ src_ size

    1 pick
    \ src_ size | src_
    1 pick
    \ src_ size | src_ size
    chars +
    \ src_ size | src_end_
    set-src-end
    \ src_ size

    drop
    \ src_
    \ rest_

    begin
        dup end? if
            exit
        endif
        \ rest_

        dup c@ 32 = if \ ' '
            \ rest_
            1 chars + ( skip char )

        else dup c@ symbol? if
            \ rest_
            dup c@
            \ rest_ | c
            char-to-s
            \ rest_ | s_ size
            print-sym-token
            1 chars +

        else dup start-with-func? if
            \ rest_
            print-func-token
            \ rest_
            4 chars +
            \ rest_

        else dup
            \ rest_ | rest_
            match-ident
            \ rest_ | index ok
        if
            \ rest_ index
            1 pick
            \ rest_ index | rest_
            1 pick
            \ rest_ index | rest_ index
            print-ident-token
            \ rest_ index
            chars +
            \ rest_

        else
            panic
        endif
        endif
        endif
        endif
    again
;

main
bye
