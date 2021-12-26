ConfigCtx.ParseOptions(args);

IWaitFor waitFor = ConfigCtx.Options.WaitFor switch
{
    WaitForEvents.PingFail => new WaitForPingFail(),
    WaitForEvents.PingSuccess => new WaitForPingSuccess(),
    WaitForEvents.TcpConnect => new WaitForTcp(),
    WaitForEvents.TcpRegexResponse => new WaitForTcp(),
    WaitForEvents.TcpSsh2Response => new WaitForTcp()
};

if (await waitFor.WaitForAsync()) {
    Environment.Exit(0);
} else {
    Environment.Exit(1);
}
