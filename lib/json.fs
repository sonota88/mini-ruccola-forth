: dq .\" \"" ;

: Json-print-int ( node_ -- )
    Node-get-int
    \ n
    print-int
;

: Json-print-str ( node_ -- )
    dq

    Node-get-str
    \ str_ size
    type

    dq
;

: Json-print-list ( list_ -- ) recursive
    \ list_

    ." ["

    dup
    \ list_ list_
    List-len
    \ list_ size

    dup 0 = if
        \ list_ size
        drop drop
        ." ]"
        cr
        exit
    endif

    0
    \ list_ size 0
    do

        dup
        \ list_ list_
        i List-get
        \ list_ node_

        i 0 > if
            ." ,"
        endif

        dup Node-get-type
        \ list_ node_ type
        dup Node-type-int = if
            ( int )
            \ list_ node_ type
            drop
            \ list_ node_
            Json-print-int
            \ list_

        else dup Node-type-str = if
            ( str )
            \ list_ node_ type
            drop
            \ list_ node_
            Json-print-str
            \ list_

        else dup Node-type-list = if
            ( list )
            \ list_ node_ type
            drop
            \ list_ node_
            Node-get-list
            \ list_ list_
            Json-print-list
            \ list_

        endif
        endif
        endif

    loop

    \ list_
    drop
    ." ]"
    cr
;

: Json-print ( list_ -- )
    Json-print-list
;

( -------------------------------- )

: consume-int ( s_ size -- node_ num-chars )
    1 pick
    1 pick
    \ s_ size s_ size
    drop-2
    \ s_ s_ size

    non-digit-index
    \ s_ index ok
    check-and-panic

    \ s_ index
    \ s_ num-chars
    swap
    \ num-chars s_
    1 pick
    \ num-chars s_ num-chars

    parse-int
    \ num-chars n

    Node-new-int
    \ num-chars node_
    swap
    \ node_ num-chars
;

: consume-str ( list_  s_ -- len-to-consume )
    dup
    \ list_ s_ s_
    1 34 char-index ( find double quote at end of string )
    \ list_ s_ index

    1 pick
    \ list_ s_ index / s_
    drop-2
    \ list_ index / s_
    1
    \ list_ index / s_ start-pos
    2 pick
    \ list_ index / s_ start-pos index
    substr
    \ list_ index / s2_

    2 pick
    \ list_ index s2_ / list_
    drop-3
    \ index s2_ / list_
    1 pick
    \ index s2_ / list_ s2_
    drop-2
    \ index / list_ s2_
    2 pick
    \ index / list_ s2_ index
    1 -
    \ index / list_ s2_ content-len
    
    List-add-str-v2
    \ index / list_

    drop
    \ index
    1 +
    \ len-to-consume
;

: Json-parse-list ( rest_ -- rest_ list_ ) recursive
    \ s_

    List-new
    \ s_ list_
    swap
    \ list_ s_

    1 chars + ( skip first '[' )
    \ list_ s_

    begin
        dup
        \ list_ s_ s_
        c@
        \ list_ s_ c

        dup 0 = if
            \ list_ s_ c
            drop drop

            panic

        else dup 91 = if \ '['
            \ list_ s_ c
            drop
            \ list_ s_
            dup
            \ list_ s_ s_
            Json-parse-list ( recursion )
            \ list_ s_ | rest_ inner-list_
            3 pick
            \ list_ s_ | rest_ inner-list_ | list_
            1 pick
            \ list_ s_ | rest_ inner-list_ | list_ inner-list_
            List-add-list
            \ list_ s_ | rest_ inner-list_ | list_
            drop drop
            \ list_ s_ rest_
            2 pick
            \ list_ s_ | rest_ list_
            drop-2
            drop-2
            \ rest_ list_
            swap
            \ list_ rest_

        else dup 93 = if \ ']'
            \ ." 124 ]" cr

            \ list_ s_ c
            drop
            \ list_ s_
            1 chars +
            \ list_ s_

            swap
            exit

        else dup 10 = if \ LF
            \ ." 137 LF" cr
            \ list_ s_ c
            drop
            \ list_ s_
            1 chars +
                    
        else dup 32 = if \ SPC
            \ list_ s_ c
            drop
            \ list_ s_
            1 chars +

        else dup 44 = if \ ','
            \ list_ s_ c
            drop
            \ list_ s_
            1 chars +

        else dup is-digit-char if
            \ list_ s_ c
            drop
            \ list_ s_

            dup
            \ list_ s_ s_
            10000 ( TODO )
            \ list_ s_ s_ size
            consume-int
            \ list_ s_ node_ num-chars

            3 pick
            \ list_ s_ node_ num-chars list_
            2 pick
            \ list_ s_ node_ num-chars list_ node_

            List-add-v2
            \ list_ s_ node_ num-chars list_

            drop
            \ list_ s_ node_ num-chars
            2 pick
            \ list_ s_ node_ num-chars s_
            1 pick
            \ list_ s_ node_ num-chars s_ num-chars

            chars +
            \ list_ s_ node_ num-chars s_+{num-chars}
            drop-1
            \ list_ s_ node_ s_+{num-chars}
            drop-1
            \ list_ s_ s_+{num-chars}
            drop-1
            \ list_ s_+{num-chars}
            \ list_ next_s_

        else dup 34 = if \ '"'
            \ list_ s_ c
            drop
            \ list_ s_

            1 pick
            1 pick
            \ list_ s_ list_ s_

            consume-str
            \ list_ s_ slen

            chars +
            \ list_ rest_

        else
            ." ("
            emit
            ." )" cr
            ." 142: must not happen"
            panic

        endif
        endif
        endif
        endif
        endif
        endif
        endif
        endif
    again

    \ list_
;

: Json-parse ( src_ -- list_ )
    Json-parse-list
    \ rest_ list_
    swap drop
    \ list_
;
