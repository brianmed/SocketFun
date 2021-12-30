using System.Net.Sockets;

public class WaitForTcpConnect : WaitFor
{
    public WaitForTcpConnect(ExitStatusZeroWhen exitStatusZeroWhen) : base(exitStatusZeroWhen)
    {
        HandledExceptions = new()
        {
            typeof(TaskCanceledException),
            typeof(TcpTimeoutException),
            typeof(SocketException)
        };

        LogPrefix = nameof(WaitForTcpConnect);
    }

    protected override async Task<bool> RunAsync()
    {
        TcpClientTimeout tcpClientTimeout = new();

        using TcpClient tcpClient = await tcpClientTimeout.ConnectAsync(
            ConfigCtx.Options.IpAddress,
            (int)ConfigCtx.Options.Port, 
            TimeSpan.FromMilliseconds(ConfigCtx.Options.TcpConnectTimeout));

        return tcpClient?.Connected ?? false;
    }
}
