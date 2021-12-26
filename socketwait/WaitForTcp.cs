using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

public class WaitForTcp : IWaitFor
{
    async public Task<bool> WaitForAsync()
    {
        TcpClientTimeout tcpClientTimeout = new();

        foreach (int _ in Enumerable.Range(0, (int)ConfigCtx.Options.TcpRetries))
        {
            try
            {
                using TcpClient tcpClient = await tcpClientTimeout.ConnectAsync(
                    ConfigCtx.Options.IpAddress,
                    (int)ConfigCtx.Options.Port, 
                    TimeSpan.FromMilliseconds(ConfigCtx.Options.TcpConnectTimeout));

                if (ConfigCtx.Options.WaitFor == WaitForEvents.TcpConnect) {
                    return true;
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
                    return false;
                }

                string pattern = ConfigCtx.Options.WaitFor switch
                {
                    WaitForEvents.TcpSsh2Response => @"^SSH-2.0",
                    WaitForEvents.TcpRegexResponse => ConfigCtx.Options.TcpRegexResponse
                };

                if (Regex.IsMatch(output, pattern)) {
                    return true;
                } else {
                    await Task.Delay((int)ConfigCtx.Options.RetryTimeout);
                }
            }
            catch (TcpTimeoutException ex)
            {
                await Task.Delay((int)ConfigCtx.Options.RetryTimeout);

                continue;
            }
            catch (SocketException ex) when (ex.Message == "Connection refused")
            {
                await Task.Delay((int)ConfigCtx.Options.RetryTimeout);

                continue;
            }
            catch (Exception)
            {
                throw;
            }
        }

        return false;
    }
}
