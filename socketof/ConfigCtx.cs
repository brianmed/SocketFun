using Microsoft.Extensions.PlatformAbstractions;

using CommandLine;
using CommandLine.Text;
using Mapster;

namespace SocketOf.ConfigCtx;

public enum SocketStates
{
    Any,
    CloseWait,
    Closed,
    Closing,
    Established,
    FinWait1,
    FinWait2,
    LastAck,
    Listening,
    SynRecv,
    SynSent,
    TimeWait
}

public class Options
{
    [Option("procDirectory", Required = false, HelpText = $"Path to proc Filesystem")]
    public string ProcDirectory { get; internal set; } = "/proc";

    [Option("quiet", Required = false, HelpText = "Exit with 0 if socket found and 1 if not")]
    public bool Quiet { get; internal set; }

    // [Option("state", Required = false, Separator = ',', HelpText = "Allowed socket states")]
    // public IEnumerable<SocketStates> SocketStates { get; internal set; } = new[] { SocketOf.ConfigCtx.SocketStates.Any } ;

    [Option("dstIp", Required = false, HelpText = "Allowed destination ip")]
    public string? DestinationIp { get; internal set; }

    [Option("dstPort", Required = false, HelpText = "Allowed destination port or port range")]
    public string? DestinationPort { get; internal set; }

    [Option("srcIp", Required = false, HelpText = "Allowed destination ip")]
    public string? SourceIp { get; internal set; }

    [Option("srcPort", Required = false, HelpText = "Allowed destination port or port range")]
    public string? SourcePort { get; internal set; }

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
            ParserResult<Options> result = parser.ParseArguments<Options>(args);

            result
                .WithParsed<Options>(options =>
                {
                    ConfigCtx.Options = options.Adapt<Options>();
                })
                .WithNotParsed(errors => DisplayHelp(result, errors));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.WriteLine($"Try '{PlatformServices.Default.Application.ApplicationName} --help' for more information.");

            Environment.Exit(1);
        }   

        if (String.IsNullOrWhiteSpace(Options.DestinationIp) &&
            String.IsNullOrWhiteSpace(Options.DestinationPort) &&
            String.IsNullOrWhiteSpace(Options.SourceIp) &&
            String.IsNullOrWhiteSpace(Options.SourcePort))
        {
            Environment.Exit(2);
        }
    }

    static int DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errors)
    {
        if (errors.Any(e => e.Tag == ErrorType.VersionRequestedError))
        {
            Console.WriteLine($"{nameof(SocketOf).ToLower()} Copyright (C) 2021 Brian Medley");
            Console.WriteLine($"Version: 1");
            Console.WriteLine($"https://github.com/brianmed/SocketFun");
        }
        else
        {
            HelpText helpText = HelpText.AutoBuild(result, h =>
                {
                    h.Copyright = $"{nameof(SocketOf).ToLower()} Copyright (C) 2021 Brian Medley";

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

            Console.WriteLine(helpText);
        }

        Environment.Exit(1);

        return 1;
    }
}
