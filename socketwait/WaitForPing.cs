using System.Net.NetworkInformation;

public class WaitForPing : IWaitFor
{
    async public Task<bool> WaitForAsync()
    {
        bool? wantSuccess = null;

        if (ConfigCtx.Options.WaitFor == WaitForEvents.PingFail) {
            wantSuccess = false;
        } else if (ConfigCtx.Options.WaitFor == WaitForEvents.PingSuccess) {
            wantSuccess = true;
        }

        foreach (uint _ in Enumerable.Range(0, (int)ConfigCtx.Options.PingRetries))
        {
            PingSender pingSender = new PingSender();

            PingReply reply = await pingSender.SendAsync();

            bool isSuccess = reply.Status == IPStatus.Success;
            bool isFail = reply.Status != IPStatus.Success;

            bool shouldRetry = wantSuccess.HasValue switch
            {
                true => (wantSuccess is true && isSuccess is false) || (wantSuccess is false && isFail is false),
                false => true
            };

            if (wantSuccess.HasValue is false) {
                wantSuccess = !isSuccess;
            }

            if (shouldRetry) {
                await Task.Delay((int)ConfigCtx.Options.RetryTimeout);
            } else {
                return true;
            }
        }

        return false;
    }
}
