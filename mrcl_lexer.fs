include lib/utils.fs

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

: start-with-func? ( rest_ -- bool )
    4
    \ rest_ 4
    s" func"

    compare \ => 等しい場合 0
    0 =
;

: start-with-main? ( rest_ -- bool )
    4
    \ rest_ 4
    s" main"

    compare
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
    91 emit \ '['

    4 pick
    print-int

    44 emit \ ','
    32 emit \ ' '
    34 emit \ '"'

    3 pick
    \ lineno kind_ size s_ size | kind_
    3 pick
    \ lineno kind_ size s_ size | kind_ size
    type

    34 emit \ '"'
    44 emit \ ','
    32 emit \ ' '
    34 emit \ '"'

    1 pick
    \ lineno kind_ size s_ size | s_
    1 pick
    \ lineno kind_ size s_ size | s_ size
    type

    34 emit \ '"'
    93 emit \ ']'
    10 emit \ LF

    drop drop drop drop drop
;

: print-func-token ( -- )
    1 s" kw" s" func" print-token
;

: print-main-token ( -- )
    1 s" ident" s" main" print-token
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

        else dup start-with-main? if
            \ rest_
            print-main-token
            \ rest_
            4 chars +
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
