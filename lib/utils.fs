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

: emit-e ( c -- )
    stderr emit-file throw
    stderr flush-file throw
;

: cr-e ( -- )
    10 emit-e
;

: type-e ( s_ size -- )
    stderr write-file throw
    stderr flush-file throw
;

: panic
    s\" \nPANIC\n" type-e

    \ https://stackoverflow.com/questions/71616920/how-can-i-exit-forth-with-a-non-zero-exit-status
    1 (bye)
;

: check-and-panic ( is-ok -- )
    if
        \ ok
    else
        panic
    endif
;

: puts-fn ( s_ size -- )
    \ s"   -->> " type-e
    \ type-e
    \ cr-e

    drop drop
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

: str-pick ( i -- s_ size )
    dup 1 +
    \ i i+1
    pick
    \ i s_
    swap
    \ s_ i
    pick
;

: str-dup ( s_ size -- s_ size  s_ size )
    1 str-pick
;

: str-drop ( s_ size -- )
    drop drop
;

: str-rest ( s_ size  num-chars -- s_ size )
    2 pick
    \ s_ size  nc | s_
    1 pick
    \ s_ size  nc | s_ nc
    chars +
    \ s_ size  nc | s2_
    2 pick
    \ s_ size  nc | s2_ size
    2 pick
    \ s_ size  nc | s2_ size nc
    -
    \ s_ size  nc | s2_ size2
    drop-2
    drop-2
    drop-2
    \ s2_ size2
;

: char-at ( s_ size  i -- c )
    drop-1
    \ s_ i
    chars +
    \ s_[i]
    c@
    \ c
;

: char-index ( s_ size  start-index char -- index flag )
    2 pick
    2 pick
    \ s_ size  start-index char | size start-index
    ?do ( start-index <= i < size )
        \ s_ size  start-index char
        3 str-pick i
        \ s_ size  start-index char | s_ size  i
        char-at
        \ s_ size  start-index char | c
        1 pick = if \ c == char
            \ s_ size  start-index char
            drop drop
            str-drop
            \ (empty)

            i true
            \ i flag
            unloop exit
        endif
    loop
    \ s_ size  start-index char
    drop drop
    str-drop
    \ (empty)

    -1
    false
    \ i flag
;

: include-char? ( s_ size  c -- flag )
    0 swap
    \ s_ size  start-index c
    char-index
    \ i flag
    drop-1
;

: digit-char? ( c -- flag )
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

: int-char? ( c -- flag )
    dup digit-char? if
        drop true
        exit
    endif

    dup 45 = if \ '-'
        drop true
        exit
    endif

    drop false
;

: non-int-index ( s_ size -- index ok )
    \ s_ size
    dup 1 + 0
    \ s_ size | size+1 0
    ?do
        \ s_ size

        dup i = if
            \ s_ size
            str-drop
            i true

            unloop exit
        endif

        1 pick
        \ s_ size | s_
        i chars +
        \ s_ size | s_[i]
        c@
        \ s_ size | c

        int-char? \ s_ size | flag
        if
            \ (continue)
        else
            \ s_ size
            str-drop
            i true

            unloop exit
        endif
    loop

    \ s_ size
    str-drop
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

: lf-index ( s_ size -- index flag )
    0 10
    \ s_ size  start-index char
    char-index
    \ index flag
;

: read-stdin-all ( -- src_ size )
    here
    \ src_
    100000 chars allot \ 多めに確保

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

: str-take-int ( s_ size -- s_ size )
    str-dup
    \ s_ size  s_ size
    non-int-index
    \ s_ size | index ok

    check-and-panic
    \ s_ size  index
    drop-1
    \ s_ num-chars
;

: str-take-str ( s_ size -- s_ size )
    str-dup
    \ s_ size | s_ size
    1 34 char-index ( find double quote at end of string )
    \ s_ size | index flag
    check-and-panic
    \ s_ size | index

    2 pick
    \ s_ size  index | s_
    1 chars +
    \ s_ size  index | s_+1
    1 pick
    \ s_ size  index | s_+1 index
    1 -
    \ s_ size  index | s_+1 index-1

    drop-2
    drop-2
    drop-2
    \ s_+1 index-1
;

: starts-with-char? ( s_ size  c -- s_ size  bool )
    2 pick c@
    \ s_ size  c c0
    =
;
