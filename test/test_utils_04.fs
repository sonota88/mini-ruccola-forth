include ../lib/utils.fs

: to-tf ( bool -- )
    if
        s" true"
    else
        s" false"
    endif
;

: test-010
    49 \ '1'
    int-char?
    \ true
    to-tf type cr
;

: test-020
    65 \ 'A'
    int-char?
    \ false
    to-tf type cr
;

: test-030
    45 \ '-'
    int-char?
    \ false
    to-tf type cr
;

\ --------------------------------

." # 1" cr
test-010

." # A" cr
test-020

." # -" cr
test-030

bye
