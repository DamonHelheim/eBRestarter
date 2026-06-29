namespace eBRestarter.Infrastructure.Adapters.Wrapper;

public interface IProcess : IDisposable
{
    StreamReader StandardOutput { get; }
    StreamReader StandardError { get; }
    string ProcessName { get; }
    IntPtr MainWindowHandle { get; }

    void WaitForExit();
    bool WaitForExit(int milliseconds);
    Task WaitForExitAsync();
}
