using System.Net;
using System.Net.Sockets;

using Microsoft.Extensions.PlatformAbstractions;

using CommandLine;
using CommandLine.Text;
using Mapster;

namespace SocketWait.ConfigCtx;

public enum WaitForEvents
{
    PingFail,
    PingFlipFlop,
    PingSuccess,
    TcpConnectFail,
    TcpConnectFlipFlop,
    TcpConnectSuccess,
    TcpRegexResponse
}

public class Options
{
    [AdaptIgnore]
    public IPAddress IpAddress { get; internal set; }

    [AdaptIgnore]
    public string Host { get; internal set; }

    [AdaptIgnore]
    public uint Port { get; internal set; }

    [Option("waitFor", Required = true, HelpText = "Event Type to Wait for.")]
    public WaitForEvents WaitFor { get; internal set; }

    [Option("tcpRetries", Required = false, HelpText = "TCP Connect Retries, Default is Int32.MaxValue")]
    public uint TcpRetries { get; internal set; } = Int32.MaxValue;

    [Option("tcpConnectTimeout", Required = false, HelpText = "TCP Connect Timeout in Milliseconds, Default is 1,000")]
    public uint TcpConnectTimeout { get; internal set; } = 1_000;

    [Option("tcpReceiveTimeout", Required = false, HelpText = "TCP Receive Timeout in Milliseconds, Default is 1,000")]
    public uint TcpReceiveTimeout { get; internal set; } = 1_000;

    [Option("tcpSendTimeout", Required = false, HelpText = "TCP Send Timeout in Milliseconds, Default is 1,000")]
    public uint TcpSendTimeout { get; internal set; } = 1_000;

    [Option("tcpRegexResponse", Required = false, HelpText = "Regex that TcpRegexResponse Event Type Waits for")]
    public string TcpRegexResponse { get; internal set; }

    [Option("tcpSendFirst", Required = false, HelpText = "Single String Will be Sent Before a Response")]
    public string TcpSendFirst { get; internal set; }

    [Option("tcpUseSslStream", Required = false, HelpText = "Use SslStream (good for https)")]
    public bool TcpUseSslStream { get; internal set; }

    [Option("pingRetries", Required = false, HelpText = "Ping Retries, Default is Int32.MaxValue")]
    public uint PingRetries { get; internal set; } = Int32.MaxValue;

    [Option("pingTimeout", Required = false, HelpText = "Ping Timeout in Milliseconds, Default is 1,000")]
    public int PingTimeout { get; internal set; } = 1_000;

    [Option("retryTimeout", Required = false, HelpText = "Global Retry Timeout in Milliseconds, Default is 1,000")]
    public uint RetryTimeout { get; internal set; } = 1_000;

    [Option("version", Required = false, HelpText = "Version Information")]
    public bool Version { get; internal set; }
}

public static class ConfigCtx
{
    public static Options? Options { get; private set; }

    public static void ParseOptions(string[] args)
    {
        try
        {
            Parser parser = new Parser(with => with.HelpWriter = null);
            ParserResult<Options> result = parser.ParseArguments<Options>(
                args.SkipWhile(v => v.Trim().StartsWith('-') is false));

            result
                .WithParsed<Options>(options =>
                {
                    ConfigCtx.Options = options.Adapt<Options>();
                })
                .WithNotParsed(errors => DisplayHelp(result, errors));

            string host = args.FirstOrDefault() ?? String.Empty;
            string port = args.Skip(1).FirstOrDefault() ?? String.Empty;

            ConfigCtx.Options.Host = host;

            if (IPAddress.TryParse(host, out IPAddress ipAddress)) {
                ConfigCtx.Options.IpAddress = ipAddress;
            } else {
                try
                {
                    if (Dns.GetHostEntry(host, AddressFamily.InterNetwork) is IPHostEntry ipHostEntry && ipHostEntry.AddressList.Any()) {
                        ConfigCtx.Options.IpAddress = ipHostEntry.AddressList.First();
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Dns Lookup Issue: {ex.Message}");

                    Environment.Exit(1);
                }
            }

            if (ConfigCtx.Options.WaitFor.ToString().StartsWith("Tcp")) {
                if (UInt32.TryParse(port, out UInt32 p)) {
                    ConfigCtx.Options.Port = p;
                } else {
                    if (String.IsNullOrWhiteSpace(port)) {
                        Console.Error.WriteLine("Please Pass in a Port");
                    } else {
                        Console.Error.WriteLine("Unable to Parse Port");
                    }
                    Environment.Exit(1);
                }
            }

            if (ConfigCtx.Options.WaitFor == WaitForEvents.TcpRegexResponse) {
                if (ConfigCtx.Options.TcpRegexResponse is null) {
                    Console.Error.WriteLine("Please pass in a --tcpRegexResponse");
                }
            } else if (ConfigCtx.Options.TcpRegexResponse is not null) {
                Console.Error.WriteLine("Please do not pass in a --tcpRegexResponse");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.WriteLine($"Try '{PlatformServices.Default.Application.ApplicationName} --help' for more information.");

            Environment.Exit(1);
        }   
    }

    static int DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errors)
    {
        if (errors.Any(e => e.Tag == ErrorType.VersionRequestedError))
        {
            Console.WriteLine($"{nameof(SocketWait).ToLower()} Copyright (C) 2021 Brian Medley");
            Console.WriteLine($"Version: 1");
            Console.WriteLine($"https://github.com/brianmed/SocketFun");
        }
        else
        {
            HelpText helpText = HelpText.AutoBuild(result, h =>
                {
                    // h.Copyright = $"{nameof(SocketWait).ToLower()} Copyright (C) 2021 Brian Medley";

                    h.Copyright = String.Empty;

                    h.AutoVersion = false;

                    h.Heading = String.Empty;

                    h.AddDashesToOption = true;

                    h.AddEnumValuesToHelpText = true;

                    return HelpText.DefaultParsingErrorsHandler(result, h);
                },
                e =>
                {
                    return e;
                },
                verbsIndex: true);

            helpText.OptionComparison = HelpText.RequiredThenAlphaComparison;

            Console.WriteLine($"{PlatformServices.Default.Application.ApplicationName} [host] [port]" + helpText);
        }

        Environment.Exit(1);

        return 1;
    }
}
