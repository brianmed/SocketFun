using System.Net;
using System.Net.Sockets;

// https://makolyte.com/how-to-set-a-timeout-for-tcpclient-connectasync/

public class TcpTimeoutException : Exception
{
    public TcpTimeoutException(string msg) : base(msg) { }
}

public class TcpClientTimeout
{
    public async Task<TcpClient> ConnectAsync(IPAddress ip, int port, TimeSpan connectTimeout)
    {
        TcpClient tcpClient = new();

        Task cancelTask = Task.Delay(connectTimeout);
        Task connectTask = tcpClient.ConnectAsync(ip, port);

        await Task.WhenAny(connectTask, cancelTask);

        if (cancelTask.IsCompleted && tcpClient.Connected is false) {
            throw new TcpTimeoutException("Timed out");
        } else {
            return tcpClient;
        }
    }
}
