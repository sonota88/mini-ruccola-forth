include lib/utils.fs
include lib/types.fs
include lib/json.fs

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

: alphabet? ( c -- bool )
    dup 97 < if \ 'a'
        drop false exit
    endif

    dup 122 > if \ 'z'
        drop false exit
    endif

    drop true
;

: ident-char? ( c -- bool )
    dup alphabet? if
        drop true exit
    endif

    \ TODO large letter

    dup 95 = if \ '_'
        drop true exit
    endif

    drop false
;

: non-ident-index ( s_ size -- index flag )
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

        ident-char? \ s_ | flag
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

: lf-index ( s_ size -- index flag )
    drop
    \ s_
    0 10
    \ s_ start-index char
    char-index
    \ index

    dup 0 >= if
        \ index
        true
        \ index ok
    else
        \ index
        false
        \ index ng
    endif
;

: match-ident ( rest_ -- num-chars flag )
    200 \ TODO dummy
    \ rest_ size
    non-ident-index
    \ index flag
    1 pick
    1 >= if
        drop true
    else
        drop false
    endif
;

: match-comment ( rest_ -- num-chars flag )
    \ rest_
    dup c@
    \ rest_ c0
    47 <> if \ '/'
        drop
        -1 false
        exit
    endif

    \ rest_
    dup 1 chars + c@
    \ rest_ c1
    47 <> if \ '/'
        drop
        -1 false
        exit
    endif

    200 \ TODO dummy
    \ rest_ size
    lf-index
    \ index flag

    1 pick 0 < if
        \ index ok
        drop false
        \ index ng
    endif
;

: start-with-func? ( rest_ -- bool )
    4
    \ rest_ size
    s" func"

    str-eq
;

: start-with-var? ( rest_ -- bool )
    3
    \ rest_ size
    s" var"

    str-eq
;

: start-with-return? ( rest_ -- bool )
    6
    \ rest_ size
    s" return"

    str-eq
;

: symbol? ( c -- bool )
    dup 40 = if \ '('
        drop
        true exit
    endif

    dup 41 = if \ ')'
        drop
        true exit
    endif

    dup 59 = if \ ';'
        drop
        true exit
    endif

    dup 123 = if \ '{'
        drop
        true exit
    endif

    dup 125 = if \ '}'
        drop
        true exit
    endif

    drop
    false
;

\ TODO mv to utils
: consume-int-v2 ( s_ size -- s_ size )
    \ TODO use str-dup
    1 pick
    1 pick
    \ s_ size  s_ size
    drop-2
    \ s_  s_ size

    non-int-index
    \ s_ index ok
    check-and-panic
    \ s_ index
    \ s_ num-chars
;

: print-token ( lineno kind_ size s_ size -- )
    List-new
    \ lineno kind_ size s_ size list_

    5 pick
    \ lineno kind_ size s_ size | list_ lineno
    List-add-int-v2
    \ lineno kind_ size s_ size | list_

    4 pick
    4 pick
    \ lineno kind_ size s_ size | list_  kind_ size
    List-add-str-v2
    \ lineno kind_ size s_ size | list_

    2 pick
    2 pick
    \ lineno kind_ size s_ size | list_  s_ size
    List-add-str-v2
    \ lineno kind_ size s_ size | list_

    Json-print-oneline
    \ lineno kind_ size s_ size

    drop
    drop
    drop
    drop
    drop

    cr
;

: print-kw-token ( val_ size -- )
    1 s" kw"
    \ val_ size | 1  kind_ size
    4 pick
    4 pick
    \ val_ size | 1  kind_ size  val_ size
    print-token
    \ val_ size

    drop
    drop
;

: print-func-token ( -- )
    s" func" print-kw-token
;

: print-var-token ( -- )
    s" var" print-kw-token
;

: print-return-token ( -- )
    s" return" print-kw-token
;

: print-ident-token ( rest_ size -- )
    1 s" ident"
    \ rest_ size | 1  kind_ size
    4 pick
    4 pick
    \ rest_ size | 1  kind_ size  rest_ size
    print-token
    \ rest_ size

    drop drop
;

: print-sym-token ( s_ size -- )
    1 s" sym"
    \ s_ size | 1 kind_ size
    4 pick
    \ s_ size | 1 kind_ size s_
    4 pick
    \ s_ size | 1 kind_ size s_ size
    print-token
    \ s_ size

    drop drop \ TODO use str-drop
;

: print-int-token ( s_ size -- )
    1 s" int"
    \ s_ size | 1 kind_ size
    4 pick
    \ s_ size | 1 kind_ size s_
    4 pick
    \ s_ size | 1 kind_ size s_ size
    print-token
    \ s_ size

    drop drop \ TODO use str-drop
;

: char-to-s ( c -- s_ size )
    s" X"
    \ c s_ size
    2 pick
    \ c s_ size | c
    2 pick
    \ c s_ size | c s_
    ! ( set char )
    \ c s_ size

    drop-2
    \ s_ size
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

        else dup c@ 10 = if \ LF
            \ rest_
            1 chars + ( skip char )

            \ TODO increment lineno

        else dup c@ symbol? if
            \ rest_
            dup c@
            \ rest_ | c
            char-to-s
            \ rest_ | s_ size
            print-sym-token
            1 chars +

        else dup c@ is-int-char? if
            \ rest_
            dup 16
            \ rest_ | rest_ dummy-size
            consume-int-v2
            \ rest_ | s_ num-chars
            1 pick \ TODO use str-dup
            1 pick
            \ rest_ | s_ num-chars | s_ num-chars
            print-int-token
            \ rest_ | s_ num-chars
            drop-1
            \ rest_ num-chars
            chars +

        else dup start-with-func? if
            \ rest_
            print-func-token
            \ rest_
            4 chars +
            \ rest_

        else dup start-with-var? if
            \ rest_
            print-var-token
            \ rest_
            3 chars +
            \ rest_

        else dup start-with-return? if
            \ rest_
            print-return-token
            \ rest_
            6 chars +
            \ rest_

        else dup
            \ rest_ | rest_
            match-ident
            \ rest_ | index flag
        if
            \ rest_ index
            1 pick
            \ rest_ index | rest_
            1 pick
            \ rest_ index | rest_ index
            print-ident-token
            \ rest_ index
            chars +
            \ rest_

        else drop dup
            \ rest_ | rest_
            match-comment
            \ rest_ | size flag
        if
            \ rest_ size
            chars +
            \ rest_

        else
            s" 275 unexpected pattern" type-e
            panic
        endif
        endif
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
