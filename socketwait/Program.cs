ConfigCtx.ParseOptions(args);

IWaitFor waitFor = ConfigCtx.Options.WaitFor switch
{
    WaitForEvents.PingFail => new WaitForPing(),
    WaitForEvents.PingFlipFlop => new WaitForPing(),
    WaitForEvents.PingSuccess => new WaitForPing(),
    WaitForEvents.TcpConnectFail => new WaitForTcpConnect(),
    WaitForEvents.TcpConnectFlipFlop => new WaitForTcpConnect(),
    WaitForEvents.TcpConnectSuccess => new WaitForTcpConnect(),
    WaitForEvents.TcpRegexResponse => new WaitForTcpRegexResponse()
};

if (await waitFor.WaitForAsync()) {
    Environment.Exit(0);
} else {
    Environment.Exit(1);
}
