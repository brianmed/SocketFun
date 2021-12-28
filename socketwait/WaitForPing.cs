using System.Net.NetworkInformation;
using System.Net.Sockets;

using Polly;

public class WaitForPing : IWaitFor
{
    async public Task<bool> WaitForAsync()
    {
        bool? wantSuccess = ConfigCtx.Options.WaitFor switch
        {
            WaitForEvents.PingFail => false,
            WaitForEvents.PingFlipFlop => null,
            WaitForEvents.PingSuccess => true,
        };

        var policyResult = await Policy<IPStatus>
            .Handle<Exception>(ex =>
            {
                if (ex is SocketException || ex is PingException) {
                    if (wantSuccess.HasValue is false) {
                        // Assume these are ping failures for FlipFlop
                        wantSuccess = true;
                    }

                    // Console.WriteLine($"{ex?.InnerException?.Message ?? ex.Message} - {wantSuccess}");

                    return wantSuccess is true;
                } else {
                    throw ex;
                }
            })
            .OrResult(ipStatus =>
            {
                bool didSucceed = ipStatus == IPStatus.Success;

                if (wantSuccess.HasValue is false) {
                    // For the FlipFLop

                    wantSuccess = !didSucceed;

                    return true;
                } else {
                    return (wantSuccess is true && didSucceed is false) ||
                        (wantSuccess is false && didSucceed is true);
                }
            })
            .WaitAndRetryAsync((int)ConfigCtx.Options.Retries, _ => TimeSpan.FromMilliseconds(ConfigCtx.Options.RetryTimeout), (result, timeSpan, retryCount, context) =>
            {
                // Console.WriteLine($"Request failed with {result.Result}. Waiting {timeSpan} before next retry. Retry attempt {retryCount}");
            })
            .ExecuteAndCaptureAsync(async () =>
            {
                PingSender pingSender = new PingSender();

                PingReply reply = await pingSender.SendAsync();

                return reply.Status;
            });

        if (policyResult.Outcome == OutcomeType.Successful) {
            return (wantSuccess is true) ?
                (policyResult.Result == IPStatus.Success) :
                (policyResult.Result != IPStatus.Success);
        } else {
            return wantSuccess is false;
        }
    }
}
