using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

public class WaitForTcp : IWaitFor
{
    async public Task<bool> WaitForAsync()
    {
        TcpClientTimeout tcpClientTimeout = new();

        /*
         * For Connecting
         */
        bool onlyWantConnect = ConfigCtx.Options.WaitFor switch
        {
            WaitForEvents.TcpConnectFail => true,
            WaitForEvents.TcpConnectFlipFlop => true,
            WaitForEvents.TcpConnectSuccess => true,
            WaitForEvents.TcpRegexResponse => false
        };

        bool needAnotherConnect = ConfigCtx.Options.WaitFor == WaitForEvents.TcpConnectFlipFlop;

        bool? wantConnectSuccess = ConfigCtx.Options.WaitFor switch
        {
            WaitForEvents.TcpConnectFail => false,
            WaitForEvents.TcpConnectFlipFlop => null,
            WaitForEvents.TcpConnectSuccess => true,
            WaitForEvents.TcpRegexResponse => true
        };

        foreach (int retryIdx in Enumerable.Range(0, (int)ConfigCtx.Options.TcpRetries))
        {
            try
            {
                if (retryIdx > 0) {
                    await Task.Delay((int)ConfigCtx.Options.RetryTimeout);
                }

                using TcpClient tcpClient = await tcpClientTimeout.ConnectAsync(
                    ConfigCtx.Options.IpAddress,
                    (int)ConfigCtx.Options.Port, 
                    TimeSpan.FromMilliseconds(ConfigCtx.Options.TcpConnectTimeout));

                if (ConfigCtx.Options.WaitFor == WaitForEvents.TcpConnectFlipFlop) {
                    if (needAnotherConnect) {
                        needAnotherConnect = false;

                        wantConnectSuccess = false;

                        continue;
                    }
                }

                if (onlyWantConnect) {
                    bool shouldRetry = wantConnectSuccess is false;

                    if (shouldRetry) {
                        continue;
                    }

                    return wantConnectSuccess is true;
                }

                tcpClient.ReceiveTimeout = (int)ConfigCtx.Options.TcpReceiveTimeout;
                tcpClient.SendTimeout = (int)ConfigCtx.Options.TcpSendTimeout;

                Stream stream = null;

                if (ConfigCtx.Options.TcpUseSslStream) {
                    stream = new SslStream(tcpClient.GetStream(), false);

                    await ((SslStream)stream).AuthenticateAsClientAsync(ConfigCtx.Options.Host);
                } else {
                    stream = tcpClient.GetStream();
                }

                if (ConfigCtx.Options.TcpSendFirst is not null) {
                    await stream.WriteAsync(Encoding.UTF8.GetBytes(ConfigCtx.Options.TcpSendFirst));
                }

                byte[] buffer = new byte[4096];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                string output = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                if (output is null) {
                    throw new ArgumentNullException($"No output received from {ConfigCtx.Options.Host}:{ConfigCtx.Options.Port}");
                }

                return Regex.IsMatch(output, ConfigCtx.Options.TcpRegexResponse);
            }
            catch (Exception ex)
            {
                if (ex is TcpTimeoutException || (ex is SocketException)) {
                    if (needAnotherConnect) {
                        needAnotherConnect = false;

                        wantConnectSuccess = true;
                    } else if (onlyWantConnect) {
                        bool shouldRetry = wantConnectSuccess is true;

                        if (shouldRetry) {
                            continue;
                        }

                        return wantConnectSuccess is false;
                    }
                } else {
                    throw;
                }
            }
        }

        return false;
    }
}
