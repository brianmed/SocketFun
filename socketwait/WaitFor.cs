using Polly;
using Polly.Retry;

public enum ExitStatusZeroWhen
{
    Success,
    Failure,
    FlipFlop
}

public abstract class WaitFor
{
    protected List<Type> HandledExceptions { get; init; }

    protected string LogPrefix { get; init; }

    protected abstract Task<bool> RunAsync();

    private ExitStatusZeroWhen ExitStatusZeroWhen { get; init; }

    private bool IsInitialRun = true;

    public WaitFor(ExitStatusZeroWhen exitStatusZeroWhen)
    {
        ExitStatusZeroWhen = exitStatusZeroWhen;
    }

    public async Task<bool> IsExitStatusZeroAsync()
    {
        using (LogContext.PushProperty("Prefix", $"{LogPrefix}"))
        {
            PolicyResult<bool> policyResult = await RunInitialAsync();

            if (ExitStatusZeroWhen != ExitStatusZeroWhen.FlipFlop) {
                bool desiredResult = ExitStatusZeroWhen == ExitStatusZeroWhen.Success
                    ? true
                    : false;

                return ExitStatusIsZero(policyResult, desiredResult);
            }

            /*
             * Else FlipFlop
             */

            using (LogContext.PushProperty("Prefix", $"{LogPrefix}.FlipFlop"))
            {
                bool desiredFlipFlopResult = !policyResult.Result;
                Log.Information($"Got {policyResult.Result}. Next is FlipFlop with Result {desiredFlipFlopResult}.");

                IsInitialRun = false;

                PolicyResult<bool> policyFlipFlop = await RunFlipFlopAsync(desiredFlipFlopResult);
                return ExitStatusIsZero(policyFlipFlop, desiredFlipFlopResult);
            }
        }
    }

    private async Task<PolicyResult<bool>> RunInitialAsync()
    {
        int retries = ExitStatusZeroWhen == ExitStatusZeroWhen.FlipFlop
            ? 0
            : (int)ConfigCtx.Options.Retries;

        HashSet<bool> allowedSuccessfulInput = new();

        if (ExitStatusZeroWhen == ExitStatusZeroWhen.Success)
        {
            allowedSuccessfulInput.Add(true);
        }
        else if (ExitStatusZeroWhen == ExitStatusZeroWhen.Failure)
        {
            allowedSuccessfulInput.Add(false);
        }
        else if (ExitStatusZeroWhen == ExitStatusZeroWhen.FlipFlop)
        {
            allowedSuccessfulInput.Add(true);
            allowedSuccessfulInput.Add(false);
        }

        return await RunPolicyAsync(retries, allowedSuccessfulInput);
    }

    private async Task<PolicyResult<bool>> RunFlipFlopAsync(bool desiredInput)
    {
        HashSet<bool> allowedSuccessfulInput = new();

        allowedSuccessfulInput.Clear();
        allowedSuccessfulInput.Add(desiredInput);

        return await RunPolicyAsync((int)ConfigCtx.Options.Retries, allowedSuccessfulInput);
    }

    private async Task<PolicyResult<bool>> RunPolicyAsync(int retries, HashSet<bool> allowedSuccessfulInput)
    {
        return await Policy<bool>
            .Handle<Exception>(ex => ShouldExceptionRetry(ex, allowedSuccessfulInput))
            .OrResult(r => ShouldResultRetry(r, allowedSuccessfulInput))
            .WaitAndRetryAsync(
                (int)ConfigCtx.Options.Retries,
                _ => TimeSpan.FromMilliseconds(ConfigCtx.Options.RetryTimeout),
                (result, timeSpan, retryCount, context) =>
            {
                string exceptionMessage = (result.Exception?.InnerException?.Message ?? result.Exception?.Message ?? String.Empty);

                if (String.IsNullOrWhiteSpace(exceptionMessage) is false)
                {
                    exceptionMessage = $" [{exceptionMessage}]";
                }

                Log.Information($"Retry attempt {retryCount}{exceptionMessage}. Waiting {timeSpan} before next retry.");
            })
            .ExecuteAndCaptureAsync(async () => await RunAsync());
    }

    private bool ShouldExceptionRetry(Exception ex, HashSet<bool> allowedSuccessfulInput)
    {
        if (HandledExceptions.Contains(ex.GetType()))
        {
            Log.Debug($"ShouldExceptionRetry: {ex.GetType().Name} {(ex.InnerException?.Message ?? ex.Message ?? String.Empty)} so {true} must be in {String.Join("|", allowedSuccessfulInput)} to retry");

            if (IsInitialRun && ExitStatusZeroWhen == ExitStatusZeroWhen.FlipFlop)
            {
                return false;
            }
            else
            {
                bool shouldRetry = (IsInitialRun && ExitStatusZeroWhen == ExitStatusZeroWhen.FlipFlop) ||
                    allowedSuccessfulInput.Contains(true);

                return shouldRetry;
            }
        }
        else
        {
            throw ex;
        }
    }

    private bool ShouldResultRetry(bool r, HashSet<bool> allowedSuccessfulInput)
    {
        Log.Debug($"ShouldResultRetry: If {r} is not in {String.Join("|", allowedSuccessfulInput)} retry");

        bool shouldRetry = allowedSuccessfulInput.Contains(r) is false;

        return shouldRetry;
    }

    private bool ExitStatusIsZero(PolicyResult<bool> policyResult, bool desiredResult)
    {
        Log.Debug($"ExitStatusIsZero: Outcome: {policyResult.Outcome} ExitStatusZeroWhen: {ExitStatusZeroWhen}: Result: {policyResult.Result} desiredResult: {desiredResult}: HadException: {policyResult.FinalException is not null}");

        if (policyResult.Outcome == OutcomeType.Successful)
        {
            return desiredResult == policyResult.Result;
        }
        else
        {
            return desiredResult is false && policyResult.FinalException is not null;
        }
    }
}
