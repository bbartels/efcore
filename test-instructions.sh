#!/bin/bash

git clean -xdf

./build.sh

rm -rf artifacts/bin/*/net481
