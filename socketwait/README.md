# Summary

socketwait is a utility that will wait for a socket event and the exit.  An event can be a ping failure, ping flip flop, ping success, tcp connect failure, tcp connect flip flop, tcp connect success, and tcp regex response.  The flip flops are when the utility gets an initial condition (either a success or failure) and then waits for the opposite.

A flip flop is believed useful when a box is running and then a shutdown command is issued.  The flop flop will wait until ping is non-responsive before exiting.

# Examples

For example, if a script should wait for a server to be responsive than this would work:

```bash
$ socketwait 192.168.1.133 --wait-for PingSuccess
$ echo "Ping was Responsive"
```

However, what if you want to wait for an unsuccessful ping?

```bash
$ socketwait 192.168.1.133 --wait-for PingFail
$ echo "Ping did not receive a Success"
```

Sometimes, waiting for a HTTP request to be responsive is needed:

```bash
$ socketwait host 80 --wait-for TcpRegexResponse --tcp-send-first "GET / HTTP/1.1\r\n" --tcp-regex-response '(?i)title' --tcp-retries 300
$ echo "HTTP response valid within 5 minutes"
```

Also, https is supported:

```bash
$ socketwait host 443 --wait-for TcpRegexResponse --tcp-send-first "GET / HTTP/1.1\r\n" --tcp-regex-response '(?i)TITLE' --tcp-retries 300 --tcp-use-ssl-stream
$ echo "HTTPS response valid within 5 minutes"
```

Wait for SSH to respond:

```bash
$ socketwait 192.168.62.133 22 --wait-for TcpRegexResponse --retries 30 --tcp-regex-response '^SSH'
$ echo "SSH is responsive"
```

Wait for Dovecot IMAP to respond:

```bash
$ socketwait 192.168.62.133 143 --wait-for TcpRegexResponse --tcp-regex-response 'Dovecot' --retries 5 
$ echo "Dovecot IMPAP is responsive"
```

Reboot a box, if needed, and wait for ssh:

```bash
#! /bin/bash

ssh 192.168.62.133 test -e /var/run/reboot_required 

if [ $? -eq 0 ]; then
    ssh 192.168.62.133 sudo shutdown -r now
    socketwait 192.168.62.133 --wait-for PingFail --retries 30 
    socketwait 192.168.62.133 22 --wait-for TcpRegexResponse --retries 30 --tcpRegexResponse '^SSH' 
fi

ssh 192.168.62.133 echo joy
```
