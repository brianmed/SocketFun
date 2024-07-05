using System.Net;
using System.Net.Sockets;

using Microsoft.Extensions.PlatformAbstractions;

using Mono.Options;
using Serilog.Events;

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
    public IPAddress IpAddress { get; internal set; }

    public string Host { get; internal set; }

    public uint Port { get; internal set; }

    public WaitForEvents WaitFor { get; internal set; }

    public int PingTimeout { get; internal set; } = 1_000;

    public int Retries { get; internal set; } = Int32.MaxValue;

    public uint RetryTimeout { get; internal set; } = 1_000;

    public uint TcpConnectTimeout { get; internal set; } = 1_000;

    public uint TcpReceiveTimeout { get; internal set; } = 1_000;

    public uint TcpSendTimeout { get; internal set; } = 1_000;

    public string TcpRegexResponse { get; internal set; }

    public string TcpSendFirst { get; internal set; }

    public bool TcpUseSslStream { get; internal set; }

    public LogEventLevel LoggingLevel { get; internal set; } = LogEventLevel.Error;

    public bool Version { get; internal set; }
}

public static class ConfigCtx
{
    public static Options? Options { get; private set; }

    public static void ParseOptions(string[] args)
    {
        try
        {
            bool showHelp = false;
            bool showVersion = false;

            ConfigCtx.Options = new();

            OptionSet optionSet = new()
            {
                $"Usage: {nameof(SocketWait).ToLower()} host port [OPTIONS]",
                "Wait for a socket event and then exit",
                "",
                "Options:",
                { "wait-for=", "Event Type to Wait for", (WaitForEvents v) => ConfigCtx.Options.WaitFor = v },
                { "ping-timeout=", "Ping Timeout in Milliseconds [default: 1000]", (string v) => ConfigCtx.Options.PingTimeout = Int32.Parse(v) },
                { "retries=", "Retry Attempts [default: Int32.MaxValue]", (string v) => ConfigCtx.Options.Retries = Int32.Parse(v) },
                { "retry-timeout=", "Retry Timeout in Milliseconds [default: 1000]", (string v) => ConfigCtx.Options.RetryTimeout = UInt32.Parse(v) },
                { "tcp-connect-timeout=", "TCP Connect Timeout in Milliseconds [default: 1000]", (string v) => ConfigCtx.Options.TcpConnectTimeout = UInt32.Parse(v) },
                { "tcp-receive-timeout=", "TCP Receive Timeout in Milliseconds [default: 1000]", (string v) => ConfigCtx.Options.TcpReceiveTimeout = UInt32.Parse(v) },
                { "tcp-send-timeout=", "TCP Send Timeout in Milliseconds [default: 1000]", (string v) => ConfigCtx.Options.TcpSendTimeout = UInt32.Parse(v) },
                { "tcp-regex-response=", "Regex that TcpRegexResponse Event Type Waits for", (string v) => ConfigCtx.Options.TcpRegexResponse = v },
                { "tcp-send-first=", "Single String Will be Sent Before a Response", (string v) => ConfigCtx.Options.TcpSendFirst = v },
                { "tcp-use-ssl-stream", "Use SslStream (good for https)", (string v) => ConfigCtx.Options.TcpUseSslStream = v != null },
                { "log-event-level", "Minimum Logging Level", (LogEventLevel v) => ConfigCtx.Options.LoggingLevel = v },
                { "version",  "Version Information", v => showVersion = v != null },
                { "help",  "Show this Message and Exit", v => showHelp = v != null }
            };

            try
            {
                optionSet.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write($"{nameof(SocketWait).ToLower()}: ");
                Console.WriteLine(e.Message);
                Console.WriteLine($"Try '{nameof(SocketWait).ToLower()} --help' for more information.");

                return;
            }

            if (showHelp)
            {
                optionSet.WriteOptionDescriptions(Console.Out);

                Environment.Exit(0);
            }

            if (showVersion)
            {
                Console.WriteLine($"{nameof(SocketWait).ToLower()} Copyright (C) 2024 Brian Medley");
                Console.WriteLine($"Version: 0.0.5");
                Console.WriteLine($"https://github.com/brianmed/SocketFun");

                Environment.Exit(0);
            }

            string host = args.FirstOrDefault() ?? String.Empty;
            string port = args.Skip(1).FirstOrDefault() ?? String.Empty;

            ConfigCtx.Options.Host = host;

            if (IPAddress.TryParse(host, out IPAddress ipAddress))
            {
                ConfigCtx.Options.IpAddress = ipAddress;
            }
            else
            {
                try
                {
                    if (Dns.GetHostEntry(host, AddressFamily.InterNetwork) is IPHostEntry ipHostEntry && ipHostEntry.AddressList.Any())
                    {
                        ConfigCtx.Options.IpAddress = ipHostEntry.AddressList.First();
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Dns Lookup Issue: {ex.Message}");

                    Environment.Exit(1);
                }
            }

            if (ConfigCtx.Options.WaitFor.ToString().StartsWith("Tcp"))
            {
                if (UInt32.TryParse(port, out UInt32 p))
                {
                    ConfigCtx.Options.Port = p;
                }
                else
                {
                    if (String.IsNullOrWhiteSpace(port))
                    {
                        Console.Error.WriteLine("Please Pass in a Port");
                    }
                    else
                    {
                        Console.Error.WriteLine("Unable to Parse Port");
                    }

                    Environment.Exit(1);
                }
            }

            if (ConfigCtx.Options.WaitFor == WaitForEvents.TcpRegexResponse)
            {
                if (ConfigCtx.Options.TcpRegexResponse is null)
                {
                    Console.Error.WriteLine("Please pass in a --tcp-regex-response");
                }
            }
            else if (ConfigCtx.Options.TcpRegexResponse is not null)
            {
                Console.Error.WriteLine("Please do not pass in a --tcp-regex-response");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.WriteLine($"Try '{PlatformServices.Default.Application.ApplicationName} --help' for more information.");

            Environment.Exit(1);
        }   
    }
}
