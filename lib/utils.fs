: dd .s cr ;

: panic
    ." PANIC"
    1 0 /
;

: char-index--end-p ( s_ len start-index char s2_ -- s_ len start-index char s2_ end-p )
    \ s_ len start-index char s2_
    4 pick
    \ s_ len start-index char s2_ s_
    4 pick
    \ s_ len start-index char s2_ s_ len
    chars +
    \ s_ len start-index char s2_ s_+len
    1 pick
    \ s_ len start-index char s2_ s_+len s2_
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

    1 0 / ( panic )
;

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

    1 0 / ( panic )
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

: str-cp ( sa_ sb_ len -- )
    \ sa_ sb_ len
    \ ." 100119 str-cp" dd
    2 pick
    \ sa_ sb_ len sa_
    swap
    \ sa_ sb_ sa_ len
    chars +
    \ sa_ sb_ sa_end_
    \ ." 100123" dd

    begin
        \ ." ----" cr
        \ ." 100126" dd

        dup
        \ sa_ sb_ sa_end_ sa_end_
        3 pick
        \ sa_ sb_ sa_end_ sa_end_ sa_
        <= if
            \ sa_ sb_ sa_end_
            0
            \ sa_ sb_ sa_end_ 0
            2 pick
            \ sa_ sb_ sa_end_ 0 sb_
            c!
            \ sa_ sb_ sa_end_
            \ ." 100140" dd
            drop drop drop
            \ ." 100142" dd
            exit
        endif

        \ sa_ sb_ sa_end_
        2 pick
        \ sa_ sb_ sa_end_ sa_
        c@
        \ sa_ sb_ sa_end_ c
        \ ." 100149" dd

        2 pick
        \ sa_ sb_ sa_end_ c sb_
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
        \ ." 100162" dd
        drop-3
        \ ." 100164" dd
        \ sa_ sb_ sa2_ sb2_ sa_end_
        drop-3
        \ sa_ sa2_ sb2_ sa_end_
        drop-3
        \ sa2_ sb2_ sa_end_
        \ ." 100172" dd
        
        \ panic
    again
;

: substr--head ( sa_ len -- new_s_ )
    \ sa_ len
    \ ." 100105" dd
    here
    \ ." 100185" dd
    \ sa_ len sb_
    1 pick
    \ sa_ len sb_ len
    1 +
    chars allot
    \ sa_ len sb_

    2 pick
    \ sa_ len sb_ s_
    1 pick
    \ sa_ len sb_ sa_ sb_
    3 pick
    \ sa_ len sb_ sa_ sb_ len
    \ ." 100200" dd
    str-cp
    \ ." 100202" dd
    \ sa_ len sb_
    swap drop
    swap drop
    \ sb_
    \ ." 100203" dd
;

: substr ( s_ from to -- new_s_ )
    \ ." 100206" dd
    \ s_ from to
    1 pick
    \ s_ from to from
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

\ TODO 終端のチェック
: non-digit-index ( s_ -- index )
    dup
    \ s_beg_ s_
    begin
        \ s_beg_ s_

        dup c@
        \ s_beg_ s_ c

        is-digit-char \ s_beg_ s_ bool
        if
        else
            \ s_beg_ s_
            swap
            \ s_ s_beg_
            -
            exit
        endif

        \ s_beg_ s_
        1 chars +
    again

    1 0 / ( panic )
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
        \ s_ size s_+1 size
        1 -
        \ s_ size s_+1 size-1
        drop-2
        drop-2
        \ s_+1 size-1

        s>number? \ d flag
        if
            \ ok
        else
            1 0 / \ panic
        endif

        \ d
        d>s

        \ s
        negate
        \ s
    else
        s>number? \ d flag
        if
            \ ok
        else
            1 0 / \ panic
        endif

        \ d
        d>s
    endif
;
