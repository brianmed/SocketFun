public interface IWaitFor
{
    public Task<bool> WaitForAsync();
}

public class WaitForFlipFlopException : Exception
{
    public WaitForFlipFlopException(string msg) : base(msg) { }
}

