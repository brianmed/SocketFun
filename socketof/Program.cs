using System.Net;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

using Microsoft.Extensions.PlatformAbstractions;

using SocketOf.ConfigCtx;

[DllImport("libc", EntryPoint = "readlink", SetLastError = true)]
static extern int ReadLink(string path, byte[] buf, int bufSize);

ConfigCtx.ParseOptions(args);

Dictionary<int, HashSet<long>> socketInodes = new();

foreach (string directory in Directory.GetDirectories(ConfigCtx.Options.ProcDirectory))
{
    string[] segments = directory
        .Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

    if (segments.Length == 2 && Int32.TryParse(segments.Last(), out int pid)) {
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
        catch (UnauthorizedAccessException ex) when (ex.Message.StartsWith("Access to the path"))
        {
            // Ignored like pidof does
            continue;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"{PlatformServices.Default.Application.ApplicationName}: {ex.Message}");

            continue;
        }

    }
}

foreach (KeyValuePair<int, HashSet<long>> pidInodes in socketInodes)
{
    PrintMatchingSocketAddresses(pidInodes.Key, pidInodes.Value, "tcp");
    PrintMatchingSocketAddresses(pidInodes.Key, pidInodes.Value, "tcp6");
}

void PrintMatchingSocketAddresses(int pid, HashSet<long> inodes, string networkType)
{
    string fdPath = Path.Combine(ConfigCtx.Options.ProcDirectory, pid.ToString(), "net", networkType);

    using (FileStream sockets = new(fdPath, FileMode.Open, FileAccess.Read, FileShare.Read))
    using (StreamReader sr = new(sockets))
    {
        while (sr.Peek() >= 0)
        {
            string line = sr.ReadLine() ?? String.Empty;

            string local_address = null;
            string remote_address = null;
            string inode = null;

            switch (networkType)
            {
                case "tcp":
                case "tcp6":
                    string[] segments = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    (local_address, remote_address, inode) = segments switch
                    {
                        _ when segments.Length >= 11 && segments[11] == "inode" => (segments[1], segments[2], segments[11]),
                        _ when segments.Length >= 11 && segments[11] != "inode" => (segments[1], segments[2], segments[9]),
                        _ => throw new ArgumentException($"Unsupported Contents Found: {line} - {line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length}")
                    };

                    break;
            }

            if (inode == "inode") {
                continue;
            } else if (inodes.Contains(long.Parse(inode))) {
                bool? shouldPrintPid = null;

                if (String.IsNullOrWhiteSpace(ConfigCtx.Options.SourceIp) is false) {
                    IPAddress localAddress = new IPAddress(local_address
                        .Split(":").First()
                        .Chunk(8)
                        .Select(v => String.Join("", new string(v))
                            .Chunk(2).Reverse()
                            .Select(v => Convert.ToByte(new string(v), 16)))
                        .SelectMany(v => v).ToArray());

		            IPAddress srcIp = IPAddress.Parse(ConfigCtx.Options.SourceIp);

                    shouldPrintPid = localAddress.Equals(srcIp);
                }

                if (String.IsNullOrWhiteSpace(ConfigCtx.Options.SourcePort) is false) {
            		uint localPort = uint.Parse(local_address.Split(":").Last(), System.Globalization.NumberStyles.AllowHexSpecifier);

                    shouldPrintPid = localPort.Equals(uint.Parse(ConfigCtx.Options.SourcePort));
                }

                if (String.IsNullOrWhiteSpace(ConfigCtx.Options.DestinationIp) is false) {
                    IPAddress remoteAddress = new IPAddress(remote_address
                        .Split(":").First()
                        .Chunk(8)
                        .Select(v => String.Join("", new string(v))
                            .Chunk(2).Reverse()
                            .Select(v => Convert.ToByte(new string(v), 16)))
                        .SelectMany(v => v).ToArray());

		            IPAddress dstIp = IPAddress.Parse(ConfigCtx.Options.DestinationIp);

                    shouldPrintPid = remoteAddress.Equals(dstIp);
                }

                if (String.IsNullOrWhiteSpace(ConfigCtx.Options.DestinationPort) is false) {
            		uint destinationPort = uint.Parse(remote_address.Split(":").Last(), System.Globalization.NumberStyles.AllowHexSpecifier);

                    shouldPrintPid = destinationPort.Equals(uint.Parse(ConfigCtx.Options.DestinationPort));
                }

                if (shouldPrintPid.Value) {
                    Console.WriteLine(pid);

                    break;
                }
            }
        }
    }
}

Environment.Exit(ConfigCtx.Options.Quiet ? 1 : 0);

List<long> FindSocketInodes(string path)
{
    List<long> inodes = new();

    foreach (string fdLink in Directory.GetFiles(path))
    {
        byte[] buffer = new byte[4096];

        if (ReadLink(fdLink, buffer, buffer.Length) < 0) {
            int errno = Marshal.GetLastPInvokeError();

            // Sometimes links go away on busy systems
            if (errno != 2) {
                throw new Exception($"Error ReadLink {fdLink}: {Marshal.GetLastPInvokeError()}");
            }
        } else {
            string link = System.Text.Encoding.UTF8.GetString(buffer) ?? String.Empty;

            if (link.StartsWith("socket:[") && link.EndsWith("]")) {
                long inode = long.Parse(link
                    .Replace("socket:[", String.Empty)
                    .Replace("]", String.Empty));

                inodes.Add(inode);
            }
        }
    }

    return inodes;
}
