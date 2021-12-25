# Summary

socketof is a Linux utility that works like pidof, except for sockets.  The pid of a
process or processes using a socket based on a few filters is displayed.

It's much simpler than other socket utilities such as ss and netstat.  And,
will never have as many features.

# Examples

```bash
$ socketof --srcPort 3000
45126
$ socketof --dstPort 8000
46553
$ socketof --dstIp ::ffff:192.168.62.133 --dstPort 8000
46553
```

# Building

```bash
$ dotnet publish -c Release -r linux-x64 /p:PublishSingleFile=true
...
$ ls -lFa bin/Release/net6.0/linux-x64/publish/socketof 
-rwxr-xr-x@ 1 bpm  staff  24095102 Dec 25 02:08 bin/Release/net6.0/linux-x64/publish/socketof*
$ 
```
