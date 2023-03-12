#!/bin/bash

set -o nounset

nn="$1"

readonly TEMP_DIR=/tmp/vm2gol-v2-forth
mkdir -p $TEMP_DIR
readonly EXP_FILE=${TEMP_DIR}/exp.txt

run_and_diff() {
  gforth test/test_utils_${nn}.fs > ${TEMP_DIR}/act.txt
  diff -u $EXP_FILE ${TEMP_DIR}/act.txt

  # cat -An $EXP_FILE
  # echo "--"
  # cat -An ${TEMP_DIR}/act.txt
}

# --------------------------------

# char-index
test_01() {
  cat <<__EOF > $EXP_FILE
2
__EOF

  run_and_diff
}

# char-index
test_02() {
  cat <<__EOF > $EXP_FILE
not found
__EOF

  run_and_diff
}

test_03() {
  : skip
}

# is-int-char?
test_04() {
  cat <<__EOF > $EXP_FILE
# 1
true
# A
false
# -
true
__EOF

  run_and_diff
}

# non-int-index
test_05() {
  cat <<__EOF > $EXP_FILE
# "1 "
found 1
# "-123 "
found 4
# "A"
found 0
# "123"
found 3
__EOF

  run_and_diff
}

# parse-int
test_06() {
  cat <<__EOF > $EXP_FILE
# "1"
1
# "-123"
-123
__EOF

  run_and_diff
}

test_${nn}
