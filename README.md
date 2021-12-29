# SocketFun

There are two utilities here.

One is
[socketof](https://github.com/brianmed/SocketFun/blob/main/socketof/README.md),
it works like pidof, except for sockets.  Only works in Linux.

The other is
[socketwait](https://github.com/brianmed/SocketFun/blob/main/socketwait/README.md).
It can wait for a specific type of event before returning.  For example, ping
success or failure are event types.  Also, a TCP connection or response can be
event types.

Both are believed useful in bash scripts.
