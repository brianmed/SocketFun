using System.Net.Sockets;

using Polly;

public class WaitForTcpConnect : IWaitFor
{
    async public Task<bool> WaitForAsync()
    {
        bool? wantSuccess = ConfigCtx.Options.WaitFor switch
        {
            WaitForEvents.TcpConnectFail => false,
            WaitForEvents.TcpConnectFlipFlop => null,
            WaitForEvents.TcpConnectSuccess => true,
        };

        var policyResult = await Policy<bool>
            .Handle<Exception>(ex =>
            {
                if (ex is TcpTimeoutException || (ex is SocketException)) {
                    if (wantSuccess is null) {
                        // Assume these are connect failures for FlipFlop
                        wantSuccess = true;
                    }

                    // Console.WriteLine($"{ex?.InnerException?.Message ?? ex.Message} - {wantSuccess}");

                    return wantSuccess is true;
                } else {
                    throw ex;
                }
            })
            .OrResult(connected =>
            {
                if (wantSuccess is null) {
                    // Setup for round-trip of FlipFlop
                    wantSuccess = !connected;
                }

                return (connected is true && wantSuccess is false) ||
                    (connected is false && wantSuccess is true);
            })
            .WaitAndRetryAsync((int)ConfigCtx.Options.Retries, _ => TimeSpan.FromMilliseconds(ConfigCtx.Options.RetryTimeout), (result, timeSpan, retryCount, context) =>
            {
                // Console.WriteLine($"Request failed with {result.Result}. Waiting {timeSpan} before next retry. Retry attempt {retryCount}");
            })
            .ExecuteAndCaptureAsync(async () =>
            {
                TcpClientTimeout tcpClientTimeout = new();

                using TcpClient tcpClient = await tcpClientTimeout.ConnectAsync(
                    ConfigCtx.Options.IpAddress,
                    (int)ConfigCtx.Options.Port, 
                    TimeSpan.FromMilliseconds(ConfigCtx.Options.TcpConnectTimeout));

                return tcpClient?.Connected ?? false;
            });

        return (wantSuccess is true) ?
            (policyResult.Result == true) :
            (policyResult.Result == false);
    }
}
