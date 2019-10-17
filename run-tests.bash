#!/bin/bash

set -e

if [[ -z $1 ]]; then
  TEST_PATH="test/"
else
  TEST_PATH="$1"
fi

if [[ -d $TEST_PATH ]]; then
  echo "=== Building tests in $TEST_PATH"
  find "$TEST_PATH" -type f -name \*.csproj -print0 |xargs -0 dotnet build
  echo "=== Running tests in $TEST_PATH"
  find "$TEST_PATH" -path "*/bin/*Test.dll" -type f -print0 |xargs -0 dotnet vstest --parallel
else
  echo "=== No tests found in $TEST_PATH"
  exit 0
fi
