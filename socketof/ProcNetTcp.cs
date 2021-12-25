using System.Net;

class ProcNetTcp
{
    public bool IsHeader { get; set; }

    public IPAddress SrcAddress { get; set; }

    public uint SrcPort { get; set; }

    public IPAddress DstAddress { get; set; }

    public uint DstPort { get; set; }

    public long Inode { get; set; }

    public ProcNetTcp(string line)
    {
        string[] segments = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (segments.Length >= 11) {
            if (segments[11] == "inode") {
                IsHeader = true;
            } else {
                string local_address = segments[1];
                string remote_address = segments[2];

                Inode = long.Parse(segments[9]);

                SrcAddress = new IPAddress(local_address
                    .Split(":").First()
                    .Chunk(8)
                    .Select(v => String.Join("", new string(v))
                        .Chunk(2).Reverse()
                        .Select(v => Convert.ToByte(new string(v), 16)))
                    .SelectMany(v => v).ToArray());

                SrcPort =
                    uint.Parse(local_address
                        .Split(":")
                        .Last(), System.Globalization.NumberStyles.AllowHexSpecifier);

                DstAddress = new IPAddress(remote_address
                    .Split(":").First()
                    .Chunk(8)
                    .Select(v => String.Join("", new string(v))
                        .Chunk(2).Reverse()
                        .Select(v => Convert.ToByte(new string(v), 16)))
                    .SelectMany(v => v).ToArray());

                DstPort =
                    uint.Parse(remote_address
                        .Split(":")
                        .Last(), System.Globalization.NumberStyles.AllowHexSpecifier);
            }
        }
    }
}
