ConfigCtx.ParseOptions(args);

IWaitFor waitFor = ConfigCtx.Options.WaitFor switch
{
    WaitForEvents.PingFail => new WaitForPing(),
    WaitForEvents.PingFlipFlop => new WaitForPing(),
    WaitForEvents.PingSuccess => new WaitForPing(),
    WaitForEvents.TcpConnectFail => new WaitForTcp(),
    WaitForEvents.TcpConnectFlipFlop => new WaitForTcp(),
    WaitForEvents.TcpConnectSuccess => new WaitForTcp(),
    WaitForEvents.TcpRegexResponse => new WaitForTcp()
};

if (await waitFor.WaitForAsync()) {
    Environment.Exit(0);
} else {
    Environment.Exit(1);
}
