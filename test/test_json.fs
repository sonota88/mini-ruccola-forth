include ../lib/utils.fs
include ../lib/types.fs
include ../lib/json.fs

: test_01
    List-new
    \ list_
    Json-print
;

: test_02
    List-new
    \ list_
    1 List-add-int-1
    \ list_
    Json-print
;

: test_03
    List-new
    \ list_
    dup s" fdsa" List-add-str-1
    \ list_
    Json-print
;

: test_04
    List-new
    \ list_
    dup -123 List-add-int-1
    \ list_
    Json-print
;

: test_05
    List-new
    \ list_
    dup 123 List-add-int-1
    \ list_
    dup s" fdsa" List-add-str-1
    \ list_
    Json-print
;

: test_06
    List-new
    \ list_
    List-new
    \ list_ list_child_
    List-add-list-1
    \ list_
    Json-print
;

: test_07
    List-new
    \ list1_

    2 List-add-int-1
    \ list1_
    s" b" List-add-str-1
    \ list1_

    List-new
    \ list1_ list2_

    1 List-add-int-1
    \ list1_ list2_

    s" a" List-add-str-1
    \ list1_ list2_

    swap
    \ list2_ list1_
    List-add-list-1
    \ list2_

    3 List-add-int-1
    \ list2_
    s" c" List-add-str-1
    \ list2_

    dup List-len
    drop
   
    Json-print
;

: test_08
    List-new
    \ list_
    dup s" 漢字" List-add-str-1
    \ list_
    Json-print
;

\ test_01
\ test_02
\ test_03
\ test_04
\ test_05
\ test_06
\ test_07
\ test_08

\ --------------------------------

: test-json
    read-stdin-all
    \ src_ size

    Json-parse
    \ list_

    Json-print
;

test-json

bye
