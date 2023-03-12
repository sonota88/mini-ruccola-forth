include ../lib/utils.fs

: test-01
    s\" fdsa"
    \ s_ size

    1 120 char-index ( find 'x' )
    \ index flag
    if
        \ ok
    else
        s" not found" type
    endif

    cr
;

\ --------------------------------

test-01

bye
