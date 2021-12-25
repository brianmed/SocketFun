using System.Net;
using System.Runtime.InteropServices;

using Microsoft.Extensions.PlatformAbstractions;

using SocketOf.ConfigCtx;

ConfigCtx.ParseOptions(args);

Dictionary<int, HashSet<long>> socketInodes = new();

foreach (string directory in Directory.GetDirectories(ConfigCtx.Options.ProcDirectory))
{
    string[] segments = directory
        .Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

    if (segments.Length != 2 || Int32.TryParse(segments.Last(), out int pid) is false) {
        continue;
    }

    string fdPath = Path.Combine(directory, "fd");

    try
    {
        foreach (long inode in FindSocketInodes(fdPath))
        {
            if (socketInodes.ContainsKey(pid) is false) {
                socketInodes.Add(pid, new());
            }

            socketInodes[pid].Add(inode);
        }
    }
    catch (UnauthorizedAccessException)
    {
        // Silenty continue, like pidof
        continue;
    }
    catch (FileNotFoundException)
    {
        // Silenty continue, like pidof
        continue;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"{PlatformServices.Default.Application.ApplicationName}: {ex}");

        continue;
    }
}

foreach (KeyValuePair<int, HashSet<long>> pidInodes in socketInodes)
{
    PrintMatchingSocketAddresses(pidInodes.Key, pidInodes.Value, "tcp");
    PrintMatchingSocketAddresses(pidInodes.Key, pidInodes.Value, "tcp6");
}

Environment.Exit(ConfigCtx.Options.Quiet ? 1 : 0);

void PrintMatchingSocketAddresses(int pid, HashSet<long> inodes, string networkType)
{
    string fdPath = Path.Combine(ConfigCtx.Options.ProcDirectory, pid.ToString(), "net", networkType);

    using (FileStream sockets = new(fdPath, FileMode.Open, FileAccess.Read, FileShare.Read))
    using (StreamReader sr = new(sockets))
    {
        while (sr.Peek() >= 0)
        {
            string line = sr.ReadLine() ?? String.Empty;

            ProcNetTcp procNetTcp = null;

            switch (networkType)
            {
                case "tcp":
                case "tcp6":
                    procNetTcp = new ProcNetTcp(line);

                    break;
            }

            if (procNetTcp.IsHeader) {
                continue;
            }

            if (inodes.Contains(procNetTcp.Inode) is false) {
                continue;
            }

            bool? shouldPrintPid = null;

            if (String.IsNullOrWhiteSpace(ConfigCtx.Options.SourceIp) is false) {
                IPAddress srcIp = IPAddress.Parse(ConfigCtx.Options.SourceIp);

                shouldPrintPid = procNetTcp.SrcAddress.Equals(srcIp);
            }

            if (String.IsNullOrWhiteSpace(ConfigCtx.Options.SourcePort) is false) {
                shouldPrintPid = procNetTcp.SrcPort.Equals(uint.Parse(ConfigCtx.Options.SourcePort));
            }

            if (String.IsNullOrWhiteSpace(ConfigCtx.Options.DestinationIp) is false) {
                IPAddress dstIp = IPAddress.Parse(ConfigCtx.Options.DestinationIp);

                shouldPrintPid = procNetTcp.DstAddress.Equals(dstIp);
            }

            if (String.IsNullOrWhiteSpace(ConfigCtx.Options.DestinationPort) is false) {
                shouldPrintPid = procNetTcp.DstPort.Equals(uint.Parse(ConfigCtx.Options.DestinationPort));
            }

            if (shouldPrintPid.Value) {
                if (ConfigCtx.Options.Quiet) {
                    Environment.Exit(0);
                } else {
                    Console.WriteLine(pid);

                    break;
                }
            }
        }
    }
}

List<long> FindSocketInodes(string path)
{
    List<long> inodes = new();

    foreach (string fdLink in Directory.GetFiles(path))
    {
        FileSystemInfo link = Directory
            .ResolveLinkTarget(fdLink, returnFinalTarget: false);

        if (link.Name.StartsWith("socket:[") && link.Name.EndsWith("]")) {
            long inode = long.Parse(link.Name
                .Replace("socket:[", String.Empty)
                    .Replace("]", String.Empty));

            inodes.Add(inode);
        }
    }

    return inodes;
}
