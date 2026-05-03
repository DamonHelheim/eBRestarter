using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS.Process;
using System.Diagnostics;

namespace eBRestarter.Infrastructure.Wrapper
{
    public partial class ProcessAdapter(Process process) : IProcess
    {
        private readonly Process _process = process ?? throw new ArgumentNullException(nameof(process));

        public StreamReader StandardOutput => _process.StandardOutput;
        public StreamReader StandardError => _process.StandardError;
        public string ProcessName => _process.ProcessName;
        public IntPtr MainWindowHandle => _process.MainWindowHandle;

        public void WaitForExit() => _process.WaitForExit();
        public bool WaitForExit(int milliseconds) => _process.WaitForExit(milliseconds);
        public Task WaitForExitAsync() => _process.WaitForExitAsync();

        private bool _disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _process.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}