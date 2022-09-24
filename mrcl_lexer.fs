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

    dup digit-char? if
        drop true exit
    endif

    dup 95 = if \ '_'
        drop true exit
    endif

    drop false
;

: non-ident-index ( s_ size -- index flag )
    \ s_ size
    dup 1 + 0
    \ s_ size | size+1 0
    ?do
        \ s_ size

        dup i
        \ s_ size | size i
        = if
            str-drop
            i true

            unloop exit
        endif

        \ s_ size

        1 pick
        \ s_ size | s_
        i chars +
        \ s_ size | s_+i
        c@
        \ s_ size | c

        ident-char? \ s_ size | flag
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

: match-sym ( rest_ size -- num-chars flag )
    ( two chars )

    1 pick
    \ rest_ size | rest_
    c@ 61 = if \ '='
        \ rest_ size
        1 pick
        \ rest_ size | rest_
        1 chars +
        \ rest_ size | rest_[1]
        c@
        \ rest_ size | c1
        61 = if \ '='
            \ rest_ size
            str-drop 2 true exit \ ==
        endif
    endif
    \ rest_ size

    1 pick
    \ rest_ size | rest_
    c@ 33 = if \ '!'
        \ rest_ size
        1 pick
        1 chars +
        c@
        61 = if \ '='
            \ rest_ size
            str-drop 2 true exit \ !=
        endif
    endif
    \ rest_ size

    ( one char )

    s" +*,;(){}="
    \ rest_ size | s_ size
    3 pick c@
    \ rest_ size | s_ size  c0
    include-char? if
        \ rest_ size
        str-drop
        1 true exit
    endif

    str-drop
    0 false
;

: match-ident ( rest_ size -- num-chars flag )
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

: match-comment ( rest_ size -- num-chars flag )
    \ rest_ size
    1 pick c@
    \ rest_ size  c0
    47 <> if \ '/'
        \ rest_ size
        str-drop
        -1 false
        exit
    endif

    \ rest_ size
    1 pick 1 chars + c@
    \ rest_ size  c1
    47 <> if \ '/'
        \ rest_ size
        str-drop
        -1 false
        exit
    endif

    \ rest_ size
    lf-index
    \ index flag

    1 pick 0 < if
        \ index ok
        drop false
        \ index ng
    endif
;

: kw? ( s_ size -- flag )
    str-dup
    \ s_ size | s_ size
    s" call" str-eq if
        \ s_ size
        str-drop
        true exit
    endif

    str-dup
    s" call_set" str-eq if
        str-drop
        true exit
    endif

    str-dup
    s" case" str-eq if
        str-drop
        true exit
    endif

    str-dup
    s" func" str-eq if
        str-drop
        true exit
    endif

    str-dup
    s" return" str-eq if
        str-drop
        true exit
    endif

    str-dup
    s" set" str-eq if
        str-drop
        true exit
    endif

    str-dup
    s" var" str-eq if
        str-drop
        true exit
    endif

    str-dup
    s" when" str-eq if
        str-drop
        true exit
    endif

    str-dup
    s" while" str-eq if
        str-drop
        true exit
    endif

    str-dup
    s" _cmt" str-eq if
        str-drop
        true exit
    endif

    \ s_ size
    str-drop
    false exit
;

: print-token ( lineno  kind_ size  s_ size -- )
    List-new
    \ lineno kind_ size s_ size list_

    5 pick
    \ lineno kind_ size s_ size | list_ lineno
    List-add-int-1
    \ lineno kind_ size s_ size | list_

    4 pick
    4 pick
    \ lineno kind_ size s_ size | list_  kind_ size
    List-add-str-1
    \ lineno kind_ size s_ size | list_

    2 pick
    2 pick
    \ lineno kind_ size s_ size | list_  s_ size
    List-add-str-1
    \ lineno kind_ size s_ size | list_

    Json-print-oneline
    \ lineno  kind_ size  s_ size

    str-drop
    str-drop
    drop

    cr
;

: print-kw ( val_ size -- )
    1 s" kw"
    \ val_ size | 1  kind_ size
    4 pick
    4 pick
    \ val_ size | 1  kind_ size  val_ size
    print-token
    \ val_ size

    str-drop
;

: print-ident ( rest_ size -- )
    1 s" ident"
    \ rest_ size | 1  kind_ size
    4 pick
    4 pick
    \ rest_ size | 1  kind_ size  rest_ size
    print-token
    \ rest_ size

    str-drop
;

: print-sym ( s_ size -- )
    1 s" sym"
    \ s_ size | 1 kind_ size
    4 pick
    \ s_ size | 1 kind_ size s_
    4 pick
    \ s_ size | 1 kind_ size s_ size
    print-token
    \ s_ size

    str-drop
;

: print-int ( s_ size -- )
    1 s" int"
    \ s_ size | 1 kind_ size
    4 pick
    \ s_ size | 1 kind_ size s_
    4 pick
    \ s_ size | 1 kind_ size s_ size
    print-token
    \ s_ size

    str-drop
;

: print-str ( s_ size -- )
    1 s" str"
    \ s_ size | 1 kind_ size
    4 pick
    \ s_ size | 1 kind_ size  s_
    4 pick
    \ s_ size | 1 kind_ size  s_ size
    print-token
    \ s_ size

    str-drop
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

: lex-spc? ( s_ size -- s_ size  bool )
    32 starts-with-char?
;

: lex-lf? ( s_ size -- s_ size  bool )
    10 starts-with-char?
;

: lex-dq? ( s_ size -- s_ size  bool )
    34 starts-with-char?
;

: lex-int? ( s_ size -- s_ size  bool )
    1 pick c@
    \ s_ size  c0
    int-char?
;

: lex-sym? ( s_ size -- s_ size  [num-chars] bool )
    \ rest_ size
    str-dup
    \ rest_ size | rest_ size
    match-sym
    \ rest_ size | num-chars flag
    dup
    if
        \ rest_ size | num-chars flag
    else
        \ rest_ size | num-chars flag
        drop drop false
        \ rest_ size  false
    endif
;

: lex-ident? ( s_ size -- s_ size  [num-chars] bool )
    \ rest_ size
    str-dup
    \ rest_ size | rest_ size
    match-ident
    \ rest_ size | num-chars flag
    dup
    if
        \ rest_ size | num-chars flag
    else
        \ rest_ size | num-chars flag
        drop drop false
        \ rest_ size  false
    endif
;

: lex-comment? ( s_ size -- s_ size  [num-chars] bool )
    \ rest_ size
    str-dup
    \ rest_ size | rest_ size
    match-comment
    \ rest_ size | num-chars flag
    dup
    if
        \ rest_ size | num-chars flag
    else
        \ rest_ size | num-chars flag
        drop drop false
        \ rest_ size  false
    endif
;

: main
    read-stdin-all-v2
    \ src_ size

    str-dup
    \ src_ size | src_ size
    chars +
    \ src_ size | src_end_
    set-src-end
    \ src_ size
    \ rest_ size

    begin
        1 pick end? if
            exit
        endif
        \ rest_ size

        lex-spc? if
            \ rest_ size
            1 ( skip char )

        else lex-lf? if
            \ rest_ size
            1 ( skip char )

            \ TODO increment lineno

        else lex-int? if
            \ rest_ size
            str-dup
            \ rest_ size | rest_ size
            str-take-int
            \ rest_ size | s_ num-chars
            str-dup
            \ rest_ size | s_ num-chars | s_ num-chars
            print-int
            \ rest_ size | s_ num-chars
            drop-1
            \ rest_ size num-chars

        else lex-dq? if
            \ rest_ size

            str-dup
            \ rest_ size | rest_ size
            str-take-str
            \ rest_ size | s_ size

            str-dup
            \ rest_ size | s_ size | s_ size
            print-str
            \ rest_ size | s_ size

            drop-1
            \ rest_ size  size
            2 +
            \ rest_ size  size+2

        else lex-sym? if
            \ rest_ size | num-chars
            2 pick
            \ rest_ size  num-chars | s_
            1 pick
            \ rest_ size  num-chars | s_ size
            print-sym
            \ rest_ size  num-chars

        else lex-ident? if
            \ rest_ size  index

            2 pick
            \ rest_ size  index | rest_
            1 pick
            \ rest_ size  index | rest_ index
            kw? if
                \ rest_ size index
                2 pick 1 pick
                \ rest_ size index | rest_ index
                print-kw
                \ rest_ size index
            else
                \ rest_ size index
                2 pick 1 pick
                \ rest_ size index | rest_ index
                print-ident
                \ rest_ size index
            endif

        else lex-comment? if
            \ rest_ size  size

        else
            \ rest_ size
            47 emit-e
            type-e
            47 emit-e
            s" 275 unexpected pattern" type-e
            panic
        endif
        endif
        endif
        endif
        endif
        endif
        endif

        \ rest_ size delta
        str-rest
        \ next_rest_ next_size
    again
;

main
bye
