using Serilog.Core;

ConfigCtx.ParseOptions(args);

LoggingLevelSwitch logEventLevel = new();

logEventLevel.MinimumLevel = ConfigCtx.Options.LoggingLevel;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.ControlledBy(logEventLevel)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Prefix", $"Initialization")
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Prefix} {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

WaitFor waitFor = ConfigCtx.Options.WaitFor switch
{
    WaitForEvents.PingFail => new WaitForPing(ExitStatusZeroWhen.Failure),
    WaitForEvents.PingFlipFlop => new WaitForPing(ExitStatusZeroWhen.FlipFlop),
    WaitForEvents.PingSuccess => new WaitForPing(ExitStatusZeroWhen.Success),
    WaitForEvents.TcpConnectFail => new WaitForTcpConnect(ExitStatusZeroWhen.Failure),
    WaitForEvents.TcpConnectFlipFlop => new WaitForTcpConnect(ExitStatusZeroWhen.FlipFlop),
    WaitForEvents.TcpConnectSuccess => new WaitForTcpConnect(ExitStatusZeroWhen.Success),
    WaitForEvents.TcpRegexResponse => new WaitForTcpRegexResponse(ExitStatusZeroWhen.Success)
};

if (await waitFor.IsExitStatusZeroAsync())
{
    Environment.Exit(0);
}
else
{
    Environment.Exit(1);
}
