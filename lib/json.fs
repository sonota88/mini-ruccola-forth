: dq .\" \"" ;

: Json-print-int ( node_ -- )
    Node-get-int
    \ n
    .
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

: Json-parse-list-v1 ( rest_ -- rest_ list_ )
    \ s_
    begin
        dup
        \ s_ s_
        c@
        \ s_ c

        dup 0 = if
            \ s_ c
            drop drop

            1 0 / ( panic )

        else dup 91 = if \ [
            \ parse_list

            \ s_ c
            \ ." 115 [" cr
            \ dd
            drop
            1 chars +
            \ dd
            \ s_

        else dup 93 = if \ ]
            \ ." 124 ]" cr

            \ s_ c
            drop
            \ s_
            1 chars +
            \ s_

            List-new

            exit
        else dup 10 = if \ LF
            \ ." 137 LF" cr
            \ s_ c
            drop
            \ s_
            \ dd
            1 chars +
                    
        else dup 32 = if \ SPC
            \ s_ c
            drop
            \ s_
            1 chars +

        else
            ." ("
            emit
            ." )" cr
            ." 142: must not happen"
            1 0 / ( panic )

        endif
        endif
        endif
        endif
        endif
    again

    \ list_
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

: Json-parse-list ( rest_ -- rest_ list_ )
    \ s_

    List-new
    \ s_ list_
    swap
    \ list_ s_

    begin
        dup
        \ list_ s_ s_
        c@
        \ list_ s_ c

        dup 0 = if
            \ list_ s_ c
            drop drop

            1 0 / ( panic )

        else dup 91 = if \ [
            \ parse_list

            \ list_ s_ c
            \ ." 115 [" cr
            \ dd
            drop
            1 chars +
            \ dd
            \ list_ s_

        else dup 93 = if \ ]
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
            \ dd
            1 chars +
                    
        else dup 32 = if \ SPC
            \ list_ s_ c
            drop
            \ list_ s_
            1 chars +

        else dup 49 = if \ 1
            \ list_ s_ c
            drop
            \ list_ s_
            1 pick
            \ list_ s_ list_
            1 List-add-int-v2
            \ list_ s_ list_
            drop
            \ list_ s_
            1 chars + ( TODO )

        else dup 34 = if \ "

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
    again

    \ list_
;

: Json-parse ( src_ -- list_ )
    Json-parse-list
    \ rest_ list_
    swap drop
    \ list_
;
