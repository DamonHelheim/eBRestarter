using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS.Process;

namespace eBRestarter.Infrastructure.Wrapper;

public class ProcessInfoService : IProcessInfoService
{
    public string GetCurrentExecutablePath()
    {
        // Gibt den Pfad der aktuell laufenden .exe zurück.
        // MainModule kann theoretisch null sein, daher das ! (Null-Forgiving),
        // da wir in einem laufenden Prozess sind.
        //Process.GetCurrentProcess().MainModule!.FileName
        return Environment.ProcessPath!;
    }
}
