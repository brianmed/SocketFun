using System.Net.NetworkInformation;

public class WaitForPingSuccess : IWaitFor
{
    async public Task<bool> WaitForAsync()
    {
        foreach (uint _ in Enumerable.Range(0, (int)ConfigCtx.Options.PingRetries))
        {
            PingSender pingSender = new PingSender();

            PingReply reply = await pingSender.SendAsync();

            if (reply.Status == IPStatus.Success) {
                return true;
            } else {
                await Task.Delay((int)ConfigCtx.Options.RetryTimeout);
            }
        }

        return false;
    }
}
