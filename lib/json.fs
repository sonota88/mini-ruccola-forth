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

: print-indent ( pretty lv -- )
    swap
    \ lv pretty
    if
        dup 0 = if
            drop exit
        else
            0
            \ lv 0
            do
                ."   "
            loop
        endif
    else
        drop
    endif
;

: Json-print-list ( list_ pretty lv -- ) recursive
    \ list_ pretty lv
    2 pick
    \ list_ pretty lv list_
    drop-3
    \ pretty lv list_

    ." ["
    2 pick if cr endif
    \ pretty lv list_

    dup
    \ pretty lv list_ | list_
    List-len
    \ pretty lv list_ | size

    dup 0 = if
        \ pretty lv list_ size
        3 pick
        3 pick
        print-indent

        drop drop drop drop
        ." ]"
        exit
    endif

    0
    \ pretty lv list_ size 0
    do

        dup
        \ pretty lv list_ list_
        i List-get
        \ pretty lv list_ node_

        i 0 > if
            ." ,"
            \ pretty lv list_ node_
            3 pick
            if
                cr
            else
                ."  "
            endif
        endif
        \ pretty lv list_ node_

        3 pick
        3 pick
        \ pretty lv list_ node_ | pretty lv
        1 +
        print-indent
        \ pretty lv list_ node_

        dup Node-get-type
        \ pretty lv list_ node_ type
        dup Node-type-int = if
            ( int )
            \ pretty lv list_ node_ type
            drop
            \ pretty lv list_ node_
            Json-print-int
            \ pretty lv list_

        else dup Node-type-str = if
            ( str )
            \ pretty lv list_ node_ type
            drop
            \ pretty lv list_ node_
            Json-print-str
            \ pretty lv list_

        else dup Node-type-list = if
            ( list )
            \ pretty lv list_ node_ type
            drop
            \ pretty lv list_ node_
            Node-get-list
            \ pretty lv list_ | list_
            3 pick
            \ pretty lv list_ | list_ pretty
            3 pick
            \ pretty lv list_ | list_ pretty lv
            1 +
            \ pretty lv list_ | list_ pretty lv+1
            Json-print-list
            \ pretty lv list_

        endif
        endif
        endif

    loop

    \ pretty lv list_
    2 pick if cr endif
    \ pretty lv list_

    2 pick
    2 pick
    print-indent
    \ pretty lv list_

    drop
    drop
    drop

    ." ]"
;

: Json-print ( list_ -- )
    true 0 Json-print-list
;

: Json-print-oneline ( list_ -- )
    false 0 Json-print-list
;

( -------------------------------- )

: consume-int ( s_ size -- node_ num-chars )
    1 pick
    1 pick
    \ s_ size s_ size
    drop-2
    \ s_ s_ size

    non-int-index
    \ s_ index ok
    check-and-panic

    \ s_ index
    \ s_ num-chars
    swap
    \ num-chars | s_
    1 pick
    \ num-chars | s_ num-chars

    parse-int
    \ num-chars n

    Node-new-int
    \ num-chars node_
    swap
    \ node_ num-chars
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

        else dup is-int-char? if
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
            \ list_ s_ node_ num-chars | list_
            2 pick
            \ list_ s_ node_ num-chars | list_ node_

            List-add-v2
            \ list_ s_ node_ num-chars | list_

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

            dup 200
            \ list_ s_ | s_ dummy-size
            take-str
            \ list_ s_ | s_ size
            3 pick
            \ list_ s_ | s_ size | list_
            2 pick
            2 pick
            \ list_ s_ | s_ size | list_  s_ size
            List-add-str-v3
            \ list_ s_ | s_ size

            drop-1
            \ list_ s_ size
            chars +
            \ list_ rest_
            2 chars +
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

: Json-parse ( src_ size -- list_ )
    drop \ TODO
    Json-parse-list
    \ rest_ list_
    swap drop
    \ list_
;
