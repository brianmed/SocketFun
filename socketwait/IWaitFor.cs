using Polly;
using Polly.Retry;

public enum ExpectedResult
{
    Success,
    Failure,
    FlipFlop
}

public interface IWaitFor
{
    public List<Type> HandledExceptions { get; set; }

    public string LogContextPrefix { get; set; }

    public Task<bool> RunAsync();

    public ExpectedResult ExpectedResult { get; set; }

    public async Task<bool> WaitForAsync()
    {
        using IDisposable _ = LogContext.PushProperty("Prefix", LogContextPrefix);

        CancellationTokenSource policyCts = new();
        Context policyContext = new("RetryContext");

        int retries = ExpectedResult == ExpectedResult.FlipFlop ?
            0 : (int)ConfigCtx.Options.Retries;

        HashSet<bool> allowedSuccessfulInput = new();

        if (ExpectedResult == ExpectedResult.Success) {
            allowedSuccessfulInput.Add(true);
        } else if (ExpectedResult == ExpectedResult.Failure) {
            allowedSuccessfulInput.Add(false);
        } else if (ExpectedResult == ExpectedResult.FlipFlop) {
            allowedSuccessfulInput.Add(true);
            allowedSuccessfulInput.Add(false);
        }

        PolicyResult<bool> policyResult = await Policy<bool>
            .Handle<Exception>(ex => ShouldExceptionRetry(ex, allowedSuccessfulInput))
            .OrResult(r => ShouldResultRetry(r, allowedSuccessfulInput))
            .WaitAndRetryAsync(retries, _ => TimeSpan.FromMilliseconds(ConfigCtx.Options.RetryTimeout), (result, timeSpan, retryCount, context) =>
            {
                string exceptionMessage = (result.Exception?.InnerException?.Message ?? result.Exception?.Message ?? String.Empty);

                if (String.IsNullOrWhiteSpace(exceptionMessage) is false) {
                    exceptionMessage = $" [{exceptionMessage}]";
                }

                Log.Information($"Retry attempt {retryCount}{exceptionMessage}. Waiting {timeSpan} before next retry.");
            })
            .ExecuteAndCaptureAsync(async (ctx, ct) => await RunAsync(), policyContext, policyCts.Token);

        if (ExpectedResult != ExpectedResult.FlipFlop) {
            bool desiredResult = ExpectedResult == ExpectedResult.Success ? true : false;

            return GetExpectedResult(policyResult, desiredResult);
        }

        /*
         * Else FlipFlop
         */

        using IDisposable __ = LogContext.PushProperty("Prefix", $"{LogContextPrefix}.FlipFlop");

        bool desiredFlipFlopResult = !policyResult.Result;

        Log.Information($"Got {policyResult.Result}. Next is FlipFlop with Result {desiredFlipFlopResult}.");

        allowedSuccessfulInput.Clear();
        allowedSuccessfulInput.Add(desiredFlipFlopResult);

        CancellationTokenSource flipFlopCts = new();
        Context flipFlopContext = new("FlipFlopContext");

        PolicyResult<bool> policyFlipFlop = await Policy<bool>
            .Handle<Exception>(ex => ShouldExceptionRetry(ex, allowedSuccessfulInput))
            .OrResult(r => ShouldResultRetry(r, allowedSuccessfulInput))
            .WaitAndRetryAsync((int)ConfigCtx.Options.Retries, _ => TimeSpan.FromMilliseconds(ConfigCtx.Options.RetryTimeout), (result, timeSpan, retryCount, context) =>
            {
                string exceptionMessage = (result.Exception?.InnerException?.Message ?? result.Exception?.Message ?? String.Empty);

                if (String.IsNullOrWhiteSpace(exceptionMessage) is false) {
                    exceptionMessage = $" [{exceptionMessage}]";
                }

                Log.Information($"Retry attempt {retryCount}{exceptionMessage}. Waiting {timeSpan} before next retry.");
            })
            .ExecuteAndCaptureAsync(async (ctx, ct) => await RunAsync(), flipFlopContext, flipFlopCts.Token);

        return GetExpectedResult(policyFlipFlop, desiredFlipFlopResult);
    }

    private bool ShouldExceptionRetry(Exception ex, HashSet<bool> allowedSuccessfulInput)
    {
        if (HandledExceptions.Contains(ex.GetType())) {
            Log.Debug($"ShouldExceptionRetry: {ex.GetType().Name} {(ex.InnerException?.Message ?? ex.Message ?? String.Empty)} so {true} must be in {String.Join("|", allowedSuccessfulInput)} to retry");

            bool shouldRetry = allowedSuccessfulInput.Contains(true);

            return shouldRetry;
        } else {
            throw ex;
        }
    }

    public bool ShouldResultRetry(bool r, HashSet<bool> allowedSuccessfulInput)
    {
        Log.Debug($"ShouldResultRetry: If {r} is not in {String.Join("|", allowedSuccessfulInput)} retry");

        bool shouldRetry = allowedSuccessfulInput.Contains(r) is false;

        return shouldRetry;
    }

    public bool GetExpectedResult(PolicyResult<bool> policyResult, bool desiredResult)
    {
        Log.Debug($"GetExpectedResult: Outcome: {policyResult.Outcome} ExpectedResult: {ExpectedResult}: Result: {policyResult.Result} desiredResult: {desiredResult}: HadException: {policyResult.FinalException is not null}");

        if (policyResult.Outcome == OutcomeType.Successful) {
            return desiredResult == policyResult.Result;
        } else {
            return desiredResult is false && policyResult.FinalException is not null;
        }
    }
}
