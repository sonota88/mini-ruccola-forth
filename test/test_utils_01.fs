include ../lib/utils.fs

: test-01
    \ index of 's' => 2
    s\" fdsa"
    \ s_ size

    1 115 char-index ( find 's' )
    \ index flag
    if
        \ ok
    else
        panic
    endif
    \ index

    print-int
    \ (empty)
    cr
;

\ --------------------------------

test-01

bye
