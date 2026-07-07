using eBRestarter.Infrastructure.Wrappers;
using System.Diagnostics;

namespace eBRestarter.Infrastructure.Adapters.Wrapper;

/// <summary>
/// Implementierung von <see cref="IProcessWrapper"/>, die statische <see cref="Process"/>-Methoden kapselt.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): KEIN ADAPTER (Fall A - Infrastruktur-Hilfsklasse / Thin Wrapper)</strong><br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 1.1 des Leitfadens ist diese Klasse <strong>kein Adapter</strong> im Sinne der hexagonalen Architektur, da sie kein Port-Interface aus dem Application Core implementiert. Sie dient als reiner technischer "Thin Wrapper" für Unittests in der Infrastruktur.<br/>
/// - <strong>Aktion:</strong> Wurde aus <c>Infrastructure/Adapters/Wrapper</c> in den Ordner <c>Infrastructure/Wrappers</c> verschoben.
/// </para>
/// </summary>
public sealed class ProcessWrapper : IProcessWrapper
{
    /// <summary>
    /// Starts a process resource specified by the <see cref="ProcessStartInfo"/> parameter
    /// and associates the resource with a new <see cref="Process"/> component.
    /// </summary>
    /// <param name="info">The <see cref="ProcessStartInfo"/> containing startup data (file name, arguments, etc.).</param>
    /// <returns>
    /// A new <see cref="Process"/> component associated with the process resource,
    /// or <c>null</c> if no process resource was started.
    /// </returns>
    public IProcess? Start(ProcessStartInfo info)
    {
        var process = Process.Start(info);

        // If the startup was successful, we wrap the real process into our adapter
        if (process is not null)
        {
            return new ProcessAdapter(process);
        }

        return null;
    }

    /// <summary>
    /// Checks whether at least one instance of a process with the specified name is currently running.
    /// </summary>
    /// <param name="name">The friendly name of the process (without the .exe extension).</param>
    /// <returns><c>true</c> if the process is running; otherwise, <c>false</c>.</returns>
    public bool IsProcessRunning(string name)
    {
        // Forwards directly to the static .NET API
        return Process.GetProcessesByName(name).Length > 0;
    }

    /// <summary>
    /// Terminates all running instances of the specified process immediately (hard kill).
    /// </summary>
    /// <param name="name">The name of the process to be terminated.</param>
    /// <remarks>
    /// This method contains <b>no exception handling</b>. Errors (e.g., "Access Denied")
    /// are passed through to the caller (the service) and must be handled there.
    /// </remarks>
    public void KillProcess(string name)
    {
        var processes = Process.GetProcessesByName(name);
        foreach (var process in processes)
        {
            process.Kill();
        }
    }

    /// <summary>
    /// Retrieves all running processes and wraps them into our testable adapters.
    /// </summary>
    public IProcess[] GetProcesses()
    {
        var processes = Process.GetProcesses();

        // RESOLUTION: We take each real process (p) and encapsulate it within the ProcessAdapter.
        // The resulting collection is then converted into an array of IProcess.
        return [.. processes.Select(p => (IProcess)new ProcessAdapter(p))];
    }
}
