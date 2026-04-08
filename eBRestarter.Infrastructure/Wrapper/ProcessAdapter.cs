using eBRestarter.Infrastructure.Wrapper.Interface;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace eBRestarter.Infrastructure.Wrapper
{
    public class ProcessAdapter(Process process) : IProcess
    {
        private readonly Process _process = process ?? throw new ArgumentNullException(nameof(process));

        public StreamReader StandardOutput => _process.StandardOutput;
        public StreamReader StandardError => _process.StandardError;
        public string ProcessName => _process.ProcessName;
        public IntPtr MainWindowHandle => _process.MainWindowHandle;

        public void WaitForExit() => _process.WaitForExit();
        public bool WaitForExit(int milliseconds) => _process.WaitForExit(milliseconds);
        public Task WaitForExitAsync() => _process.WaitForExitAsync();

        public void Dispose() => _process.Dispose();
    }
}