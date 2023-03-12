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

: print-indent-0 ( pretty lv -- )
    swap
    \ lv pretty
    if
        dup 0 = if
            drop exit
        else
            0
            \ lv 0
            ?do
                ."   "
            loop
        endif
    else
        drop
    endif
;

: print-indent-2 ( pretty lv -- pretty lv )
    1 pick
    1 pick
    \ pretty lv pretty lv
    print-indent-0
    \ pretty lv
;

: print-indent-plus1-2 ( pretty lv -- pretty lv )
    1 pick
    1 pick
    \ pretty lv | pretty lv
    1 +
    \ pretty lv | pretty lv+1
    print-indent-0
    \ pretty lv
;

: int-node? ( node_ -- node_ bool )
    dup Node-get-type
    \ node_ type
    Node-type-int =
;

: str-node? ( node_ -- node_ bool )
    dup Node-get-type Node-type-str =
;

: list-node? ( node_ -- node_ bool )
    dup Node-get-type Node-type-list =
;

defer Json-print-list

: Json-print-node ( pretty lv node_ -- pretty lv )
    \ pretty lv node_

    int-node? if
        \ pretty lv node_
        Json-print-int
        \ pretty lv

    else str-node? if
        \ pretty lv node_
        Json-print-str
        \ pretty lv

    else list-node? if
        \ pretty lv node_
        Node-get-list
        \ pretty lv | list_
        2 pick
        \ pretty lv | list_ pretty
        2 pick
        \ pretty lv | list_ pretty lv
        1 +
        \ pretty lv | list_ pretty lv+1
        Json-print-list
        \ pretty lv

    else
        panic
    endif
    endif
    endif
    \ pretty lv
;

( Json-print-list )
:noname ( list_ pretty lv -- )
    ." ["
    1 pick if cr endif
    \ list_ pretty lv

    2 pick
    \ list_ pretty lv | list_
    List-len
    \ list_ pretty lv | size

    dup 0 = if
        \ list_ pretty lv size
        2 pick
        2 pick
        print-indent-0
        \ list_ pretty lv size

        drop drop drop drop
        ." ]"
        exit
    endif

    0
    \ list_ pretty lv size 0
    ?do
        \ list_ pretty lv

        i 0 > if
            ." ,"
            \ list_ pretty lv
            1 pick
            if
                cr
            else
                ."  "
            endif
        endif
        \ list_ pretty lv

        print-indent-plus1-2
        \ list_ pretty lv

        2 pick
        \ list_ pretty lv list_
        i List-get
        \ list_ pretty lv node_

        Json-print-node
        \ list_ pretty lv
    loop

    \ list_ pretty lv
    1 pick if cr endif
    \ list_ pretty lv

    print-indent-2
    \ list_ pretty lv

    drop
    drop
    drop

    ." ]"
;
is Json-print-list

: Json-print ( list_ -- )
    true 0 Json-print-list
;

: Json-print-oneline ( list_ -- )
    false 0 Json-print-list
;

( -------------------------------- )

: take-int ( s_ size -- node_ num-chars )
    str-take-int
    \ s_ size

    str-dup
    \ s_ size  s_ size
    parse-int
    \ s_ size  n

    Node-new-int
    \ s_ size  node_

    drop-2
    \ size node_
    swap
    \ node_ size
    \ node_ num-chars
;

: Json-parse-int ( list_  s_ size -- list_  s_ size )
    \ list_  s_ size

    str-dup
    \ list_  s_ size | s_ size
    take-int
    \ list_  s_ size | node_ num-chars

    4 pick
    \ list_  s_ size  node_ num-chars | list_
    2 pick
    \ list_  s_ size  node_ num-chars | list_ node_

    List-add-1
    \ list_  s_ size  node_ num-chars | list_

    drop
    \ list_  s_ size  node_ num-chars

    3 str-pick
    \ list_  s_ size  node_ num-chars | s_ size
    2 pick
    \ list_  s_ size  node_ num-chars | s_ size num-chars
    str-rest
    \ list_  s_ size  node_ num-chars | s_+n size-n
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
    str-take-str
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

: json-list-beg? ( s_ size -- s_ size bool )
    91
    \ s_ size  '['
    starts-with-char?
;

: json-list-end? ( s_ size -- s_ size bool )
    93
    \ s_ size  ']'
    starts-with-char?
;

: json-lf? ( s_ size -- s_ size bool )
    10 starts-with-char?
;

: json-spc? ( s_ size -- s_ size bool )
    32 starts-with-char?
;

: json-comma? ( s_ size -- s_ size bool )
    44 starts-with-char?
;

: json-dq? ( s_ size -- s_ size bool )
    34 starts-with-char?
;

: json-int? ( s_ size -- s_ size bool )
    1 pick c@
    int-char?
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

        json-list-beg? if \ '['
            \ list_  s_ size
            Json-parse-list ( recursion )
            \ list_ | rest_ size  inner-list_

            3 pick
            \ list_ | rest_ size  inner-list_ | list_
            1 pick
            \ list_ | rest_ size  inner-list_ | list_ inner-list_
            List-add-list-0
            \ list_ | rest_ size  inner-list_

            drop
            \ list_  rest_ size

        else json-list-end? if \ ']'
            \ list_  s_ size
            1 str-rest
            \ list_  s_+1 size-1
            2 pick
            \ list_  s_+1 size-1  list_
            drop-3
            \ s_+1 size-1  list_
            exit

        else json-lf? if \ LF
            \ list_  s_ size
            1 str-rest
            \ list_  s_+1 size-1

        else json-spc? if \ SPC
            1 str-rest

        else json-comma? if \ ','
            1 str-rest

        else json-dq? if \ '"'
            \ list_  s_ size
            Json-parse-str
            \ list_ s_+n size-n

        else json-int? if
            \ list_  s_ size
            Json-parse-int
            \ list_ s_+n size-n

        else
            ." ("
            emit
            ." )" cr
            ." must not happen"
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
