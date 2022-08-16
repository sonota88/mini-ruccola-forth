create read-char-buf_ 1 chars allot

: read-char ( -- char num-read )
    read-char-buf_ 1 stdin read-file throw
    \ n
    dup
    \ n n
    0 = if
        0
        \ n 0
    else
        read-char-buf_
        \ n read-char-buf_
        c@
        \ n c
        swap
        \ c n
    endif
;

\ --------------------------------

: dd .s cr ;

: emit-e ( c -- )
    stderr emit-file
    stderr flush-file
;

: cr-e ( -- )
    10 emit-e
;

: type-e ( s_ size -- )
    stderr write-file throw
    stderr flush-file
;

: panic
    s\" \nPANIC\n" type-e

    \ https://stackoverflow.com/questions/71616920/how-can-i-exit-forth-with-a-non-zero-exit-status
    1 (bye)
;

\ TODO throw が使える？
: check-and-panic ( is-ok -- )
    if
        \ ok
    else
        panic
    endif
;

: print-int--positive ( s -- )
    \ https://gforth.org/manual/Formatted-numeric-output.html
    0
    <<#
    #s
    #>
    type
    #>>
;

: print-int--negative ( s -- )
    negate
    45 emit \ '-'
    print-int--positive
;

: print-int ( s -- )
    dup
    0 >= if
        print-int--positive
    else
        print-int--negative
    endif
;

: char-index--end-p ( s_ len start-index char s2_ -- s_ len start-index char s2_ end-p )
    \ s_ len start-index char s2_
    4 pick
    \ s_ len start-index char s2_ | s_
    4 pick
    \ s_ len start-index char s2_ | s_ len
    chars +
    \ s_ len start-index char s2_ | s_+len
    1 pick
    \ s_ len start-index char s2_ | s_+len s2_
    <=
    \ s_ len start-index char s2_ flag
;

: char-index-v1 ( s_ len start-index char -- index )
    3 pick
    \ s_ len start-index char s_
    2 pick
    \ s_ len start-index char s_ start-index
    chars +
    \ s_ len start-index char s2_

    begin
        \ s_ len start-index char s2_

        char-index--end-p if ( s_ + len < s2_ )
            -1
            exit
        endif

        dup c@
        \ s_ len start-index char s2_ c

        2 pick = if
            \ s_ len start-index char s2_
            swap drop
            \ s_ len start-index s2_
            swap drop
            \ s_ len s2_
            swap drop
            \ s_ s2_

            swap
            \ s2_ s_
            -
            \ index
            exit
        else
            \ s_ len start-index char s2_
            1 chars +
            \ s_ len start-index char s2_
        endif
    again

    panic
;

\ index に加えて flag を返すとよい？
: char-index ( s_ start-index char -- index )
    2 pick
    \ s_ start-index char s_
    2 pick
    \ s_ start-index char s_ start-index
    chars +
    \ s_ start-index char s2_

    begin
        \ s_ start-index char s2_

        dup c@
        \ s_ start-index char s2_ c

        dup 0 = if ( end of string )
            -1
            exit
        endif


        2 pick = if
            \ s_ start-index char s2_
            swap drop
            \ s_ start-index s2_
            swap drop
            \ s_ s2_

            swap
            \ s2_ s_
            -
            \ index
            exit
        else
            \ s_ start-index char s2_
            1 chars +
            \ s_ start-index char s2_
        endif
    again

    panic
;

: drop-1 ( 1 0 -- 0 )
    swap drop
;

: drop-2 ( 2 1 0 -- 1 0 )
    >r >r
    drop
    r> r>
;

: drop-3 ( 3 2 1 0 -- 2 1 0 )
    >r >r >r
    drop
    r> r> r>
;

: str-eq ( sa_ size  sb_ size -- bool )
    compare \ => 等しい場合 0
    0 =
;

: str-cp ( sa_ sb_ len -- )
    \ sa_ sb_ len
    \ ." str-cp" dd
    2 pick
    \ sa_ sb_ len sa_
    swap
    \ sa_ sb_ sa_ len
    chars +
    \ sa_ sb_ sa_end_

    begin
        dup
        \ sa_ sb_ sa_end_ | sa_end_
        3 pick
        \ sa_ sb_ sa_end_ | sa_end_ sa_
        <= if
            \ sa_ sb_ sa_end_
            0
            \ sa_ sb_ sa_end_ | 0
            2 pick
            \ sa_ sb_ sa_end_ | 0 sb_
            c!
            \ sa_ sb_ sa_end_
            drop drop drop
            exit
        endif

        \ sa_ sb_ sa_end_
        2 pick
        \ sa_ sb_ sa_end_ sa_
        c@
        \ sa_ sb_ sa_end_ c

        2 pick
        \ sa_ sb_ sa_end_ | c sb_
        c!
        \ sa_ sb_ sa_end_

        2 pick
        \ sa_ sb_ sa_end_ sa_
        1 chars +
        \ sa_ sb_ sa_end_ sa2_
        
        2 pick
        \ sa_ sb_ sa_end_ sa_ sb_
        1 chars +
        \ sa_ sb_ sa_end_ sa2_ sb2_
        2 pick
        \ sa_ sb_ sa_end_ sa2_ sb2_ sa_end_
        drop-3
        \ sa_ sb_ sa2_ sb2_ sa_end_
        drop-3
        \ sa_ sa2_ sb2_ sa_end_
        drop-3
        \ sa2_ sb2_ sa_end_
        
        \ panic
    again
;

: substr--head ( sa_ len -- new_s_ )
    \ sa_ len
    here
    \ sa_ len sb_
    1 pick
    \ sa_ len sb_ len
    1 +
    chars allot
    \ sa_ len sb_

    2 pick
    \ sa_ len sb_ | s_
    1 pick
    \ sa_ len sb_ | sa_ sb_
    3 pick
    \ sa_ len sb_ | sa_ sb_ len
    str-cp
    \ sa_ len sb_
    swap drop
    swap drop
    \ sb_
;

: substr ( s_ from to -- new_s_ )
    \ s_ from to
    1 pick
    \ s_ from | to from
    -
    \ s_ from len

    2 pick
    \ s_ from len s_
    2 pick
    \ s_ from len s_ from
    chars +
    \ s_ from len tail_
    swap
    \ s_ from tail_ len
    substr--head
    \ s_ from new_s_
    swap drop
    \ s_ new_s_
    swap drop
    \ new_s_
;

: str-len ( s_ -- len )
    \ s_
    dup
    \ s_ s2_
    begin
        dup c@
        \ s_ s2_ c
        0 = if
            \ s_ s2_
            swap -
            exit
        else
            \ s_ s2_
            1 chars +
        endif
    again
;

: is-digit-char ( c -- bool )
    dup 45 = if \ '-'
        drop true
        exit
    endif

    dup 48 < if \ '0'
        drop false
        exit
    endif

    dup 57 > if \ '9'
        drop false
        exit
    endif

    drop true
;

: non-digit-index ( s_ size -- index ok )
    \ s_ size
    0
    \ s_ | size 0
    ?do
        \ s_
        dup
        \ s_ | s_
        i chars +
        \ s_ | s_+i
        c@
        \ s_ | c

        is-digit-char \ s_ | flag
        if
            \ (continue)
        else
            \ s_
            drop
            i true

            unloop exit
        endif
    loop

    \ s_
    drop
    -1 false
;

: parse-int ( s_ size -- n )
    1 pick
    \ s_ size s_
    c@
    \ s_ size c
    45 = if \ '-'
        \ s_ size
        1 pick
        \ s_ size s_
        1 chars +
        \ s_ size s_+1
        1 pick
        \ s_ size s_+1 | size
        1 -
        \ s_ size s_+1 size-1
        drop-2
        drop-2
        \ s_+1 size-1

        s>number? \ d flag
        check-and-panic

        \ d
        d>s

        \ s
        negate
        \ s
    else
        s>number? \ d flag
        check-and-panic

        \ d
        d>s
    endif
;

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
