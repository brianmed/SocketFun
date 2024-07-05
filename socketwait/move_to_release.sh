#! /bin/bash

set -e

mv -i publish/linux-arm/socketwait publish/socketwait-linux-arm
mv -i publish/linux-arm64/socketwait publish/socketwait-linux-arm64
mv -i publish/linux-x64/socketwait publish/socketwait-linux-x64
mv -i publish/osx-universal/socketwait publish/socketwait-macos-universal
mv -i publish/win-x64/socketwait.exe publish/socketwait-win-x64.exe

echo Done
