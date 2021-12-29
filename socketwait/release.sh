#! /bin/bash

set -e

dotnet publish -c Release -r osx-x64 /p:PublishSingleFile=true --output publish/osx-x64
dotnet publish -c Release -r linux-x64 /p:PublishSingleFile=true --output publish/linux-x64
dotnet publish -c Release -r linux-arm64 /p:PublishSingleFile=true --output publish/linux-arm64
dotnet publish -c Release -r linux-arm /p:PublishSingleFile=true --output publish/linux-arm
dotnet publish -c Release -r win-x64 /p:PublishSingleFile=true --output publish/win-x64

mv -i publish/linux-arm/socketwait publish/socketwait-linux-arm
mv -i publish/linux-arm64/socketwait publish/socketwait-linux-arm64
mv -i publish/linux-x64/socketwait publish/socketwait-linux-x64
mv -i publish/osx-x64/socketwait publish/socketwait-osx-x64
mv -i publish/win-x64/socketwait.exe publish/socketwait-win-x64.exe

echo Done
