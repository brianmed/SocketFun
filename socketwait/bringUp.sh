#! /bin/bash

ssh 192.168.62.133 test -e /var/run/reboot_required 

if [ $? -eq 0 ]; then
    ssh 192.168.62.133 sudo shutdown -r now
    socketwait 192.168.62.133 --waitFor PingFail --pingRetries 30 
    socketwait 192.168.62.133 22 --waitFor TcpRegexResponse --tcpRetries 30 --tcpRegexResponse '^SSH' 
fi

ssh 192.168.62.133 echo joy
