using System.Net.NetworkInformation;

public class WaitForPingFail : IWaitFor
{
    async public Task<bool> WaitForAsync()
    {
        foreach (uint _ in Enumerable.Range(0, (int)ConfigCtx.Options.PingRetries))
        {
            PingSender pingSender = new PingSender();

            PingReply reply = await pingSender.SendAsync();

            if (reply.Status == IPStatus.Success) {
                await Task.Delay((int)ConfigCtx.Options.RetryTimeout);
            } else {
                return true;
            }
        }

        return false;
    }
}
