# Summary

socketof is a utility that works like pidof, except for sockets.  The pid of a
process or processes using a socket based on a few filters is displayed.

It's much simpler than other socket utilities such as ss and netstat.

# Examples

```bash
$ socketof --srcPort 3000
45126
$ socketof --dstPort 8000
46553
$ socketof --dstIp ::ffff:192.168.62.133 --dstPort 8000
46553
```
