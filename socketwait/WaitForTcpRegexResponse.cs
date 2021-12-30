using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

public class WaitForTcpRegexResponse : WaitFor
{
    public WaitForTcpRegexResponse(ExitStatusZeroWhen exitStatusZeroWhen) : base(exitStatusZeroWhen)
    {
        HandledExceptions = new()
        {
            typeof(TaskCanceledException),
            typeof(TcpTimeoutException),
            typeof(SocketException)
        };

        LogPrefix = nameof(WaitForTcpRegexResponse);
    }

    protected override async Task<bool> RunAsync()
    {
        TcpClientTimeout tcpClientTimeout = new();

        using TcpClient tcpClient = await tcpClientTimeout.ConnectAsync(
            ConfigCtx.Options.IpAddress,
            (int)ConfigCtx.Options.Port, 
            TimeSpan.FromMilliseconds(ConfigCtx.Options.TcpConnectTimeout));

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
}
