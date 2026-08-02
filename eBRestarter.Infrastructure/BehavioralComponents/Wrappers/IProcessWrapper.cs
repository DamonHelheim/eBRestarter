using System.Diagnostics;

namespace eBRestarter.Infrastructure.BehavioralComponents.Wrappers;

public interface IProcessWrapper
{
    IProcess? Start(ProcessStartInfo info);

    // For IsProcessAlive
    bool IsProcessRunning(string name);

    // For CloseApplication
    void KillProcess(string name);

    IProcess[] GetProcesses();
}

