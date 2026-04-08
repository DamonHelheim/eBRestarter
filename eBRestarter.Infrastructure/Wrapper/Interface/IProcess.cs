using System;
using System.IO;
using System.Threading.Tasks;

namespace eBRestarter.Infrastructure.Wrapper.Interface
{
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
}