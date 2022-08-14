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
    dup c@ 102 <> if
        \ 1文字目が f ではない
        drop false exit
    endif

    \ rest_
    1 chars +
    \ rest_

    dup c@ 117 <> if
        \ 1文字目が u ではない
        drop false exit
    endif
    
    \ rest_
    1 chars +
    \ rest_

    dup c@ 110 <> if
        \ 1文字目が n ではない
        drop false exit
    endif
    
    \ rest_
    1 chars +
    \ rest_

    dup c@ 99 <> if
        \ 1文字目が c ではない
        drop false exit
    endif

    \ rest_
    drop true
;

: print-func-token ( -- )
    91 emit \ '['
    49 emit \ '1'
    44 emit \ ','
    32 emit \ ' '
    34 emit \ '"'
    107 emit \ 'k'
    119 emit \ 'w'
    34 emit \ '"'
    44 emit \ ','
    32 emit \ ' '
    34 emit \ '"'
    102 emit \ 'f'
    117 emit \ 'u'
    110 emit \ 'n'
    99 emit \ 'c'
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

        else dup start-with-func? if
            \ rest_
            print-func-token
            \ rest_
            4 chars +
            \ rest_

        else
            panic
        endif
        endif
    again
;

main
bye
