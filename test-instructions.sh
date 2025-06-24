#!/bin/bash

git clean -xdf

rm -rf ~/.nuget
./build.sh

rm -rf artifacts/bin/*/net481
