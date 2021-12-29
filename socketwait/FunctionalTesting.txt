| Event         | Original Power State | Final Power State | Exception was Last | Result |
| ------------- | -------------------- | ----------------- | ------------------ | ------ |
| PingFail      | On                   | On                | True               | Rare   |
| PingFail      | On                   | On                | False              | [ ]    |
| PingFail      | On                   | Off               | True               | Hard   |
| PingFail      | On                   | Off               | False              | [ ]    |
| PingFail      | Off                  | On                | True               | DC     |
| PingFail      | Off                  | On                | False              | DC     |
| PingFail      | Off                  | Off               | True               | [ ]    |
| PingFail      | Off                  | Off               | False              | [ ]    |


| Event         | Original Power State | Final Power State | Exception was Last | Result |
| ------------- | -------------------- | ----------------- | ------------------ | ------ |
| PingFlipFlop  | On                   | On                | True               | Rare   |
| PingFlipFlop  | On                   | On                | False              | [ ]    |
| PingFlipFlop  | On                   | Off               | True               | Rare   |
| PingFlipFlop  | On                   | Off               | False              | [ ]    |
| PingFlipFlop  | Off                  | On                | True               | Rare   |
| PingFlipFlop  | Off                  | On                | False              | [ ]    |
| PingFlipFlop  | Off                  | Off               | True               | [ ]    |
| PingFlipFlop  | Off                  | Off               | False              | [ ]    |


| Event         | Original Power State | Final Power State | Exception was Last | Result |
| ------------- | -------------------- | ----------------- | ------------------ | ------ |
| PingSuccess   | On                   | On                | True               | Rare   |
| PingSuccess   | On                   | On                | False              | [ ]    |
| PingSuccess   | On                   | Off               | True               | DC     |
| PingSuccess   | On                   | Off               | False              | DC     |
| PingSuccess   | Off                  | On                | True               | Rare   |
| PingSuccess   | Off                  | On                | False              | [ ]    |
| PingSuccess   | Off                  | Off               | True               | [ ]    |
| PingSuccess   | Off                  | Off               | False              | [ ]    |


| Event          | Original Socket State | Final Socket State | Exception was Last | Result |
| -------------  | --------------------- | ------------------ | ------------------ | ------ |
| TcpConnectFail | On                    | On                 | True               | Rare   |
| TcpConnectFail | On                    | On                 | False              | [ ]    |
| TcpConnectFail | On                    | Off                | True               | [ ]    |
| TcpConnectFail | On                    | Off                | False              | Rare?  |
| TcpConnectFail | Off                   | On                 | True               | DC     |
| TcpConnectFail | Off                   | On                 | False              | DC     |
| TcpConnectFail | Off                   | Off                | True               | [ ]    |
| TcpConnectFail | Off                   | Off                | False              | Rare?  |


| Event              | Original Socket State | Final Socket State | Exception was Last | Result |
| ------------------ | --------------------- | ------------------ | ------------------ | ------ |
| TcpConnectFlipFlop | On                    | On                 | True               | Rare   |
| TcpConnectFlipFlop | On                    | On                 | False              | [ ]    |
| TcpConnectFlipFlop | On                    | Off                | True               | [ ]    |
| TcpConnectFlipFlop | On                    | Off                | False              | Rare?  |
| TcpConnectFlipFlop | Off                   | On                 | True               | Rare   |
| TcpConnectFlipFlop | Off                   | On                 | False              | [ ]    |
| TcpConnectFlipFlop | Off                   | Off                | True               | [ ]    |
| TcpConnectFlipFlop | Off                   | Off                | False              | Rare?  |


| Event              | Original Socket State | Final Socket State | Exception was Last | Result |
| ------------------ | --------------------- | ------------------ | ------------------ | ------ |
| TcpConnectSuccess  | On                    | On                 | True               | Rare   |
| TcpConnectSuccess  | On                    | On                 | False              | [ ]    |
| TcpConnectSuccess  | On                    | Off                | True               | DC     |
| TcpConnectSuccess  | On                    | Off                | False              | DC     |
| TcpConnectSuccess  | Off                   | On                 | True               | Rare?  |
| TcpConnectSuccess  | Off                   | On                 | False              | [ ]    |
| TcpConnectSuccess  | Off                   | Off                | True               | [ ]    |
| TcpConnectSuccess  | Off                   | Off                | False              | Rare?  |


| Event - SSH              | Original Socket State | Final Socket State  | Exception was Last | Result |
| ------------------------ | --------------------- | ------------------- | ------------------ | ------ |
| TcpConnectRegexResponse  | On                    | On                  | True               | Rare   |
| TcpConnectRegexResponse  | On                    | On                  | False              | [ ]    |
| TcpConnectRegexResponse  | On                    | Off                 | True               | DC     |
| TcpConnectRegexResponse  | On                    | Off                 | False              | DC     |
| TcpConnectRegexResponse  | Off                   | On                  | True               | Rare?  |
| TcpConnectRegexResponse  | Off                   | On                  | False              | [ ]    |
| TcpConnectRegexResponse  | Off                   | Off                 | True               | [ ]    |
| TcpConnectRegexResponse  | Off                   | Off                 | False              | Rare?  |

Ssl - HTTPS - Worked

IMAP - Worked
