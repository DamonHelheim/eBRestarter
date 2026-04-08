using System.Diagnostics;

namespace eBRestarter.Infrastructure.Wrapper.Interface;

public interface IProcessWrapper
{
    // Muss Process? zurückgeben, damit wir Streams lesen können (für MSI)
    IProcess? Start(ProcessStartInfo info);

    // Für IsProcessAlive
    bool IsProcessRunning(string name);

    // Für CloseApplication
    void KillProcess(string name);

    // Für CloseAllOpenPrograms (holt ALLE Prozesse)
    //Process[] GetProcesses();

    IProcess[] GetProcesses();

}
