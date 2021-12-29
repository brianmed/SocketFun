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

For example, socketwait can be ran in a bash script and can block until a
server answers a ping or SSH is functional.

Both are believed useful in bash scripts.
