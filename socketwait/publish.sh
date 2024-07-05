#! /bin/bash

set -e

if [ "$(uname -s)" == "Darwin" ]; then
    dotnet publish -c Release -r osx-arm64 /p:PublishAot=true --output publish/osx-arm64
    dotnet publish -c Release -r osx-x64 /p:PublishAot=true --output publish/osx-x64

    (cd publish && mkdir osx-universal && lipo -create -output osx-universal/socketwait osx-x64/socketwait osx-arm64/socketwait)
elif [ "$(uname -s)" == "Linux" ]; then
    dotnet publish -c Release -r linux-arm /p:PublishAot=true --output publish/linux-arm
    dotnet publish -c Release -r linux-arm64 /p:PublishAot=true --output publish/linux-arm64
    dotnet publish -c Release -r linux-x64 /p:PublishAot=true --output publish/linux-x64
fi

# dotnet publish -c Release -r win-x64 /p:PublishAot=true --output publish/win-x64

echo Done
