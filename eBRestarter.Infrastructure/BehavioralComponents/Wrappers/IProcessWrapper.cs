using System.Diagnostics;

namespace eBRestarter.Infrastructure.BehavioralComponents.Wrappers;

public interface IProcessWrapper
{
    // Muss Process? zurÃ¼ckgeben, damit wir Streams lesen kÃ¶nnen (fÃ¼r MSI)
    IProcess? Start(ProcessStartInfo info);

    // FÃ¼r IsProcessAlive
    bool IsProcessRunning(string name);

    // FÃ¼r CloseApplication
    void KillProcess(string name);

    IProcess[] GetProcesses();
}

