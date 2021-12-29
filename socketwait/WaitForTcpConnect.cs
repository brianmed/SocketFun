using System.Net.Sockets;

public class WaitForTcpConnect : IWaitFor
{
    public List<Type> HandledExceptions { get; set; } = new()
    {
        typeof(TaskCanceledException),
        typeof(TcpTimeoutException),
        typeof(SocketException)
    };

    public string LogContextPrefix { get; set; } = nameof(WaitForTcpConnect);

    public ExpectedResult ExpectedResult { get; set; }

    async public Task<bool> RunAsync()
    {
        TcpClientTimeout tcpClientTimeout = new();

        using TcpClient tcpClient = await tcpClientTimeout.ConnectAsync(
            ConfigCtx.Options.IpAddress,
            (int)ConfigCtx.Options.Port, 
            TimeSpan.FromMilliseconds(ConfigCtx.Options.TcpConnectTimeout));

        return tcpClient?.Connected ?? false;
    }
}
