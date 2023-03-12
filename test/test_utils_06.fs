include ../lib/utils.fs

: print-result ( s -- )
    print-int
    cr
;

: test-010
    s" 1"
    \ s_ size
    parse-int
    \ n
    print-result
;

: test-020
    s" -123"
    \ s_ size
    parse-int
    \ n
    print-result
;

\ --------------------------------

.\" # \"1\"" cr
test-010

.\" # \"-123\"" cr
test-020

bye
