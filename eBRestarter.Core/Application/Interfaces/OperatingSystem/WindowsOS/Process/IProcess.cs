namespace eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS.Process;

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
