#!/bin/bash
input=$( cat Test/TestResults/**/*.xml | grep name=\"API\" )
reg=$(echo "$input" | grep -o -E '?[0-9]*\.?[0-9]*')
num=$(echo ${reg:2:1})
cov=$(echo ${reg:2:2})

if [[ "$num" == "8" ]]; then
    exit 0
fi
if [[ "$num" == "9" ]]; then
    exit 0
fi

echo "Coverage $cov%. Less than 80% code coverage!"
exit 1