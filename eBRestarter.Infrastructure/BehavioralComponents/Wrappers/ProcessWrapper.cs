using System;
using System.Diagnostics;

namespace eBRestarter.Infrastructure.BehavioralComponents.Wrappers;

/// <summary>
/// Infrastructure Wrapper Component: Provides process creation, query, and termination operations wrapping system diagnostics.
/// </summary>
public sealed class ProcessWrapper : IProcessWrapper
{
    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Retrieves all running processes and wraps them into our testable adapters.
    /// </summary>
    public IProcess[] GetProcesses()
    {
        var processes = Process.GetProcesses();

        // ✅ .NET 10 Hot-Path Performance: Array.ConvertAll anstelle von LINQ-Allokationen
        return Array.ConvertAll(processes, static process => (IProcess)new ProcessAdapter(process));
    }

    /// <summary>
    /// Checks whether at least one instance of a process with the specified name is currently running.
    /// </summary>
    /// <param name="name">The friendly name of the process (without the .exe extension).</param>
    /// <returns><c>true</c> if the process is running; otherwise, <c>false</c>.</returns>
    public bool IsProcessRunning(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var processes = Process.GetProcessesByName(name);
        try
        {
            return processes.Length > 0;
        }
        finally
        {
            foreach (var process in processes)
            {
                process.Dispose();
            }
        }
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
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var processes = Process.GetProcessesByName(name);
        foreach (var process in processes)
        {
            try
            {
                process.Kill();
            }
            finally
            {
                process.Dispose();
            }
        }
    }

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
        ArgumentNullException.ThrowIfNull(info);

        var process = Process.Start(info);

        // If the startup was successful, we wrap the real process into our adapter
        return process is not null ? new ProcessAdapter(process) : null;
    }
}
