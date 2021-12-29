using System.Net.NetworkInformation;
using System.Net.Sockets;

public class WaitForPing : IWaitFor
{
    public List<Type> HandledExceptions { get; set; } = new()
    {
        typeof(PingException),
        typeof(SocketException),
        typeof(TaskCanceledException)
    };

    public string LogContextPrefix { get; set; } = nameof(WaitForPing);

    public ExpectedResult ExpectedResult { get; set; }

    async public Task<bool> RunAsync()
    {
        PingSender pingSender = new PingSender();

        PingReply reply = await pingSender.SendAsync();

        return reply.Status == IPStatus.Success;
    }
}
