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

: Json-parse-int ( list_  s_ size -- list_  s_ size )
    \ list_  s_ size

    str-dup
    \ list_  s_ size | s_ size
    consume-int
    \ list_  s_ size | node_ num-chars

    4 pick
    \ list_ s_ size  node_ num-chars | list_
    2 pick
    \ list_ s_ size  node_ num-chars | list_ node_

    List-add-1
    \ list_ s_ size  node_ num-chars | list_

    drop
    \ list_ s_ size node_ num-chars

    3 str-pick
    \ list_ s_ size node_ num-chars | s_ size
    2 pick
    \ list_ s_ size node_ num-chars | s_ size num-chars
    str-rest
    \ list_ s_ size node_ num-chars | s_+n size-n
    drop-2
    drop-2
    drop-2
    drop-2
    \ list_ s_+n size-n
;

: Json-parse-str ( list_  s_ size -- list_  s_ size )
    \ list_  s_ size

    str-dup
    \ list_  s_ size  s_ size
    take-str
    \ list_ s_ size | s_ size
    4 pick
    \ list_ s_ size | s_ size | list_

    2 str-pick
    \ list_ s_ size | s_ size | list_  s_ size
    List-add-str-0
    \ list_ s_ size | s_ size

    \ list_ s_ size | s_ size
    drop-1
    \ list_ s_ size  size
    2 +
    \ list_ s_ size  size+2
    str-rest
    \ list_ s_+n size-n
;

: Json-parse-list ( rest_ size -- rest_ size  list_ ) recursive
    \ s_ size

    List-new
    \ s_ size  list_
    2 str-pick
    \ s_ size | list_  s_ size
    drop-3
    drop-3
    \ list_  s_ size

    1 str-rest ( skip first '[' )
    \ list_  s_ size

    begin
        \ list_  s_ size

        1 pick
        \ list_  s_ size  s_
        c@
        \ list_  s_ size  c

        dup 91 = if \ '['
            \ list_  s_ size  c
            drop
            \ list_  s_ size
            str-dup
            \ list_  s_ size | s_ size
            Json-parse-list ( recursion )
            \ list_  s_ size | rest_ size  inner-list_

            5 pick
            \ list_ s_ size | rest_ size  inner-list_ | list_
            1 pick
            \ list_ s_ size | rest_ size  inner-list_ | list_ inner-list_
            List-add-list-1
            \ list_ s_ size | rest_ size  inner-list_ | list_

            drop drop
            \ list_  s_ size | rest_ size
            drop-2
            drop-2
            \ list_  rest_ size

        else dup 93 = if \ ']'
            \ list_  s_ size  c
            drop
            \ list_  s_ size
            1 str-rest
            \ list_  s_+1 size-1
            2 pick
            \ list_  s_+1 size-1  list_
            drop-3
            \ s_+1 size-1  list_
            exit

        else dup 10 = if \ LF
            \ list_  s_ size  c
            drop
            \ list_  s_ size
            1 str-rest
            \ list_  s_+1 size-1
                    
        else dup 32 = if \ SPC
            \ list_  s_ size  c
            drop
            \ list_  s_ size
            1 str-rest
            \ list_  s_+1 size-1

        else dup 44 = if \ ','
            \ list_  s_ size  c
            drop
            \ list_  s_ size
            1 str-rest
            \ list_  s_+1 size-1

        else dup int-char? if
            \ list_  s_ size  c
            drop
            \ list_  s_ size
            Json-parse-int
            \ list_ s_+n size-n

        else dup 34 = if \ '"'
            \ list_  s_ size  c
            drop
            \ list_  s_ size
            Json-parse-str
            \ list_ s_+n size-n

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

        \ list_  rest_ size
    again
;

: Json-parse ( src_ size -- list_ )
    Json-parse-list
    \ rest_ size  list_
    drop-1
    drop-1
    \ list_
;
