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

IWaitFor waitFor = ConfigCtx.Options.WaitFor switch
{
    WaitForEvents.PingFail => new WaitForPing() { ExpectedResult = ExpectedResult.Failure },
    WaitForEvents.PingFlipFlop => new WaitForPing() { ExpectedResult = ExpectedResult.FlipFlop },
    WaitForEvents.PingSuccess => new WaitForPing() { ExpectedResult = ExpectedResult.Success },
    WaitForEvents.TcpConnectFail => new WaitForTcpConnect() { ExpectedResult = ExpectedResult.Failure },
    WaitForEvents.TcpConnectFlipFlop => new WaitForTcpConnect() { ExpectedResult = ExpectedResult.FlipFlop },
    WaitForEvents.TcpConnectSuccess => new WaitForTcpConnect() { ExpectedResult = ExpectedResult.Success },
    WaitForEvents.TcpRegexResponse => new WaitForTcpRegexResponse() { ExpectedResult = ExpectedResult.Success }
};

if (await waitFor.WaitForAsync()) {
    Environment.Exit(0);
} else {
    Environment.Exit(1);
}
