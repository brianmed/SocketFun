# Summary

socketwait is a utility that will wait for a socket event and the exit.  An event can be PingFail, PingSuccess, TcpConnect, TcpRegexResponse, TcpSsh2Response.

# Examples

For example, if a script should wait for a server to be responsive than this would work:

```bash
$ socketwait 192.168.1.133 --waitFor PingSuccess
$ echo "Ping was Responsive"
```

However, what if you want to wait for an unsuccessful ping?

```bash
$ socketwait 192.168.1.133 --waitFor PingFail
$ echo "Ping did not receive a Success"
```

Sometimes, waiting for a HTTP request to be responsive is needed:

```bash
$ socketwait host 80 --waitFor TcpRegexResponse --tcpSendFirst "GET / HTTP/1.1\r\n" --tcpRegexResponse '(?i)title' --tcpRetries 300
$ echo "HTTP response valid within 5 minutes"
```

Also, https is supported:

```bash
$ socketwait host 443 --waitFor TcpRegexResponse --tcpSendFirst "GET / HTTP/1.1\r\n" --tcpRegexResponse '(?i)TITLE' --tcpRetries 300 --tcpUseSslStream
$ echo "HTTPS response valid within 5 minutes"
```
