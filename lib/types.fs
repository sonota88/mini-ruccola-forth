(
node / サイズは2固定でよい
0 type
1 val: int | str_ | list_
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
    \ .s cr
    1 cells
    \ val node_ 1
    +
    \ val node_+1
    !
;

: Node-new-int ( n -- node_ )
    \ ." Node-new-int" cr

    here
    \ n node_
    \ .s cr

    \ TODO 3 に統一したい
    2 cells allot
    \ n node_

    dup
    \ n node_ node_
    Node-type-int
    \ n node_ node_ type
    Node-set-type
    \ n node_

    dup
    \ n node_ node_
    2 pick
    \ n node_ node_ n

    Node-set-val
    \ n node_

    swap drop
;

(
0 type
1 str_
2 size
)
: Node-new-str ( s_ size -- node_ )
    \ ." Node-new-str" cr

    here
    \ s_ size node_
    \ .s cr

    3 cells allot
    \ s_ size node_

    1 pick
    \ s_ size node_ size
    1 pick
    \ s_ size node_ size node_
    2 cells +
    \ s_ size node_ size node_+2
    !
    \ s_ size node_
    swap drop

    \ s_ node_
    dup
    \ s_ node_ node_
    Node-type-str
    \ s_ node_ node_ type
    Node-set-type
    \ s_ node_

    dup
    \ s_ node_ node_
    2 pick
    \ s_ node_ node_ s_

    Node-set-val
    \ s_ node_

    swap drop
    \ node_
;

: Node-new-list ( list_ -- node_ )
    \ ." Node-new-int" cr

    here
    \ list_ node_
    \ .s cr

    \ TODO 3 に統一したい
    2 cells allot
    \ list_ node_

    dup
    \ list_ node_ node_
    Node-type-list
    \ list_ node_ node_ type
    Node-set-type
    \ list_ node_

    dup
    \ list_ node_ node_
    2 pick
    \ list_ node_ node_ list_

    Node-set-val
    \ list_ node_

    swap drop
    \ node_
;

: Node-get-int ( node_ -- n )
    1 cells +
    \ node_+1
    @
;

: Node-get-str ( node_ -- str_ size )
    dup 1 cells +
    @
    \ node_ str_
    swap
    \ str_ node_

    2 cells +
    @
    \ str_ size
;

: Node-get-list ( node_ -- list_ )
    1 cells +
    \ node_+1
    @
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

: List-add-int ( list_ -- list_ )
    \ list_
    dup
    \ list_ list_
    @
    \ list_ size
    1 +
    \ list_ size
    swap !
;

( deprecated / Use v2 / サイズを変更していない )
: List-add ( list_ node_ -- list_ )
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
    List-add

    dup
    \ list_ list_
    List-increment-size
    \ list_
;

: List-add-int-v2 ( list_ n -- list_ )
    \ ." List-add-int-v2" cr
    
    Node-new-int
    \ list_ node_

    ( set node )
    List-add
    \ list_

    ( increment size )
    dup dup
    \ list_ list_ list_
    List-len
    \ list_ list_ size
    1 +
    swap
    \ list_ size list_
    !
    \ list_
;

: List-add-str ( list_ s_ -- list_ )
    \ ." List-add-str" cr

    \ list_ s_
    dup str-len
    \ list_ s_ size
    \ cr ." 100263" dd
    Node-new-str \ TODO size が必要 ( s_ size -- node_ )
    \ ." 100265" dd
    \ list_ node_
    \ 100 dump panic

    ( set node )
    List-add
    \ list_

    ( increment size )
    dup dup
    \ list_ list_ list_
    List-len
    \ list_ list_ size
    1 +
    swap
    \ list_ size list_
    !
    \ list_
;

: List-add-str-v2 ( list_ s_ len -- list_ )
    \ list_ s_ len
    Node-new-str
    \ list_ node_

    ( set node )
    List-add
    \ list_

    ( increment size )
    dup dup
    \ list_ list_ list_
    List-len
    \ list_ list_ size
    1 +
    swap
    \ list_ size list_
    !
    \ list_
;

: List-add-list ( list_parent_ list_child_ -- list_parent_ )
    \ ." List-add-list" cr
    
    Node-new-list
    \ list_parent_ node_

    ( set node )
    List-add
    \ list_parent_

    ( increment size )
    dup dup
    \ list_parent_ list_parent_ list_parent_

    List-len
    \ list_parent_ list_parent_ size

    1 +
    swap
    \ list_parent_ size list_parent_
    !
    \ list_parent_
;

: List-get ( list_ n -- node_ )
    1 +
    \ list_ n+1
    cells +
    \ list_+(n+1)cells
    @
;
