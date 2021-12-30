using System.Net.NetworkInformation;
using System.Net.Sockets;

public class WaitForPing : WaitFor
{
    public WaitForPing(ExitStatusZeroWhen exitStatusZeroWhen) : base(exitStatusZeroWhen)
    {
        HandledExceptions = new()
        {
            typeof(PingException),
            typeof(SocketException),
            typeof(TaskCanceledException)
        };

        LogPrefix = nameof(WaitForPing);
    }

    protected override async Task<bool> RunAsync()
    {
        PingSender pingSender = new PingSender();

        PingReply reply = await pingSender.SendAsync();

        return reply.Status == IPStatus.Success;
    }
}
