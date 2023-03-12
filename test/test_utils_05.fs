include ../lib/utils.fs

: print-result ( n ok -- )
    if
        ." found "
    else
        ." not found "
    endif

    print-int
    cr
;

: test-010
    s" 1 "
    \ s_ size
    non-int-index
    \ index ok
    print-result
;

: test-020
    s" -123 "
    non-int-index
    print-result
;

: test-030
    s" A"
    non-int-index
    print-result
;

: test-040
    s" 123"
    non-int-index
    print-result
;

\ --------------------------------

.\" # \"1 \"" cr
test-010

.\" # \"-123 \"" cr
test-020

.\" # \"A\"" cr
test-030

.\" # \"123\"" cr
test-040

bye
