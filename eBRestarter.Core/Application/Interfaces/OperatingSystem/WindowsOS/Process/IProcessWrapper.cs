using System.Diagnostics;

namespace eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS.Process;

public interface IProcessWrapper
{
    // Muss Process? zurückgeben, damit wir Streams lesen können (für MSI)
    IProcess? Start(ProcessStartInfo info);

    // Für IsProcessAlive
    bool IsProcessRunning(string name);

    // Für CloseApplication
    void KillProcess(string name);

    IProcess[] GetProcesses();
}
