using System.Net.NetworkInformation;
using System.Text;

public class PingSender
{
    async public Task<PingReply> SendAsync()
    {
        Ping pingSender = new Ping();
        PingOptions options = new PingOptions();

        // Use the default Ttl value which is 128,
        // but change the fragmentation behavior.
        options.DontFragment = true;

        // Create a buffer of 32 bytes of data to be transmitted.
        string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        byte[] buffer = Encoding.ASCII.GetBytes(data);
        int timeout = ConfigCtx.Options.PingTimeout;

        return pingSender.Send(ConfigCtx.Options.IpAddress, timeout, buffer, options);
    }
}

