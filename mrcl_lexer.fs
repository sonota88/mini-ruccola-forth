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

create buf_ 1 chars allot

: read-char ( -- char num-read )
    buf_ 1 stdin read-file throw
    \ n
    dup
    \ n n
    0 = if
        0
        \ n 0
    else
        buf_
        \ n buf_
        c@
        \ n c
        swap
        \ c n
    endif
;

\ --------------------------------

: read-stdin-all-v2 ( -- src_ size )
    here
    \ src_
    1000 chars allot

    dup
    \ src_ src_
    \ src_ src_current_

    begin
        read-char
        \ src_ src_cur_ | char num-read
        0 = if
            \ src_ src_cur_ char
            drop
            \ src_ src_cur_

            dup
            \ src_ src_cur_ | src_cur_
            2 pick
            \ src_ src_cur_ | src_cur_ src_
            -
            \ src_ src_cur_ | size

            drop-1
            \ src_ size

            exit
        endif

        \ src_ src_cur_ | char
        1 pick
        \ src_ src_cur_ | char src_cur_
        c!
        \ src_ src_cur_

        1 +
        \ src_ src_next_
    again
;

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

: print-sym-token ( sym-c -- )
    91 emit \ '['

    1 print-int

    44 emit \ ','
    32 emit \ ' '
    34 emit \ '"'

    s" sym"
    type

    34 emit \ '"'
    44 emit \ ','
    32 emit \ ' '
    34 emit \ '"'

    emit

    34 emit \ '"'
    93 emit \ ']'
    10 emit \ LF
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

        else dup c@ 40 = if \ '('
            \ rest_
            dup c@ print-sym-token
            1 chars +

        else dup c@ 41 = if \ ')'
            \ rest_
            dup c@ print-sym-token
            1 chars +

        else dup c@ 123 = if \ '{'
            \ rest_
            dup c@ print-sym-token
            1 chars +

        else dup c@ 125 = if \ '}'
            \ rest_
            dup c@ print-sym-token
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
        endif
        endif
        endif
    again
;

main
bye
