(
node
0 type
1 val: int | str_ | list_
2 string size
)

: Node-type-int  1 ;
: Node-type-str  2 ;
: Node-type-list 3 ;

: Node-set-type ( node_ type -- )
    swap
    \ type node_
    !
;

: Node-get-type ( node_ -- type )
    @
;

: Node-set-val ( node_ val -- )
    \ ." Node-set-val" cr

    swap
    \ val node_
    1 cells
    \ val node_ 1
    +
    \ val node_+1
    !
;

: Node-get-val ( node_ -- val )
    1 cells +
    \ node_+1
    @
    \ val
;

: Node-new ( node-type -- node_ )
    here
    \ nt node_

    3 cells allot
    \ nt node_

    dup
    \ nt node_ | node_
    2 pick
    \ nt node_ | node_ nt
    Node-set-type
    \ nt node_

    drop-1
    \ node_
;

: Node-new-int ( n -- node_ )
    \ ." Node-new-int" cr

    Node-type-int Node-new
    \ n | node_

    dup
    \ n node_ | node_
    2 pick
    \ n node_ | node_ n
    Node-set-val
    \ n node_

    drop-1
    \ node_
;

: Node-set-strsize ( node_ size -- )
    swap
    \ size node_
    2 cells +
    \ size node_+2
    !
    \ (empty)
;

(
0 type
1 str_
2 size
)
: Node-new-str ( s_ size -- node_ )
    \ ." Node-new-str" cr

    Node-type-str Node-new
    \ s_ size | node_

    ( set string size )
    dup
    \ s_ size node_ | node_
    2 pick
    \ s_ size node_ | node_ size
    Node-set-strsize
    \ s_ size node_

    drop-1
    \ s_ node_

    ( set string )
    dup
    \ s_ node_ | node_
    2 pick
    \ s_ node_ | node_ s_
    Node-set-val
    \ s_ node_

    drop-1
    \ node_
;

: Node-new-list ( list_ -- node_ )
    \ ." Node-new-int" cr

    Node-type-list Node-new
    \ list_ node_

    dup
    \ list_ node_ | node_
    2 pick
    \ list_ node_ | node_ list_
    Node-set-val
    \ list_ node_

    drop-1
    \ node_
;

: Node-get-int ( node_ -- n )
    Node-get-val
;

: Node-get-str ( node_ -- str_ size )
    dup
    Node-get-val
    \ node_ str_

    swap
    \ str_ node_

    2 cells +
    @
    \ str_ size
;

: Node-get-list ( node_ -- list_ )
    Node-get-val
;

\ --------------------------------

: List-new ( -- list_ )
    here
    \ list_
    64 cells allot
    \ list_
    dup 0
    \ list_ list_ 0
    swap
    \ list_ 0 list_
    ! ( set size )
    \ list_
;

: List-len ( list_ -- size )
    0 + @
;

: List-add-v1 ( list_ node_ -- list_ )
    swap
    \ node_ list_

    dup dup
    \ node_ list_ list_ list_
    @
    \ node_ list_ list_ size
    cells +
    \ node_ list_ list_+size

    1 cells +
    \ node_ list_ list_+size+1
    \ node_ list_ dest_

    2 pick
    \ node_ list_ dest_ node_

    swap
    \ node_ list_ node_ dest_
    !
    \ node_ list_

    swap drop
    \ list_
;

: List-increment-size ( list_ -- )
    dup
    \ list_ list_
    List-len
    \ list_ size
    1 +
    \ list_ size+1
    swap
    \ size+1 list_
    ! ( set new size )
    \
;

: List-add-v2 ( list_ node_ -- list_ )
    List-add-v1

    dup
    \ list_ list_
    List-increment-size
    \ list_
;

: List-add-int-v2 ( list_ n -- list_ )
    \ ." List-add-int-v2" cr
    
    Node-new-int
    \ list_ node_
    List-add-v2
    \ list_
;

: List-add-str ( list_ s_ -- list_ )
    \ ." List-add-str" cr

    \ list_ s_
    dup str-len
    \ list_ s_ size
    Node-new-str \ TODO size が必要 ( s_ size -- node_ )
    \ list_ node_
    \ 100 dump panic

    List-add-v2
    \ list_
;

: List-add-str-v2 ( list_ s_ len -- list_ )
    \ list_ s_ len
    Node-new-str
    \ list_ node_
    List-add-v2
    \ list_
;

: List-add-str-v3 ( list_ s_ len -- )
    List-add-str-v2
    \ list_
    drop
    \ (empty)
;

: List-add-list ( list_parent_ list_child_ -- list_parent_ )
    \ ." List-add-list" cr
    
    Node-new-list
    \ list_parent_ node_
    List-add-v2
    \ list_parent_
;

: List-add-list-v2 ( list_parent_ list_child_ -- )
    List-add-list
    \ list_parent_
    drop
    \ (empty)
;

: List-get ( list_ n -- node_ )
    1 +
    \ list_ n+1
    cells +
    \ list_+(n+1)cells
    @
;

: List-get-int ( list_ n -- n )
    List-get
    \ node_
    Node-get-int
    \ n
;

: List-get-str ( list_ n -- s_ size )
    List-get
    \ node_
    Node-get-str
    \ s_ size
;

: List-get-list ( list_ n -- child_list_ )
    List-get
    \ node_
    Node-get-list
    \ child_list_
;

: List-add-all-nodrop ( list1_ list2_ -- list1_ )
    dup List-len 0
    \ list1_ list2_ | size 0
    ?do
        \ list1_ list2_
        dup i
        \ list1_ list2_ | list2_ i
        List-get
        \ list1_ list2_ | node_
        2 pick
        \ list1_ list2_ | node_ list1_
        swap
        \ list1_ list2_ | list1_ node_
        List-add-v2
        \ list1_ list2_ | list1_
        drop
        \ list1_ list2_
    loop

    drop
;

: List-rest ( list_ -- rest_list_ )
    List-new
    \ list_ newlist_
    1 pick
    \ list_ newlist_ | list_
    List-len 1
    \ list_ newlist_ | size 1
    ?do
        1 pick i
        \ list_ newlist_ | list_ i
        List-get
        \ list_ newlist_ | node_
        List-add-v2
        \ list_ newlist_
    loop

    drop-1
    \ newlist_
;
