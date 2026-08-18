using System;
using System.Diagnostics;

namespace eBRestarter.Infrastructure.BehavioralComponents.Wrappers;

/// <summary>
/// Infrastructure Wrapper Component: Provides process creation, query, and termination operations wrapping system diagnostics.
/// <para>
/// <strong>Architecture Classification: INFRASTRUCTURE WRAPPER</strong><br/>
/// - <strong>Role &amp; Responsibility:</strong> Encapsulates <see cref="Process"/> static and instance operations behind the <see cref="IProcessWrapper"/> interface for testability.<br/>
/// - <strong>Implemented Interface:</strong> <see cref="IProcessWrapper"/>.<br/>
/// </para>
/// </summary>
public sealed class ProcessWrapper : IProcessWrapper
{
    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════

    /// <inheritdoc />
    public IProcess[] GetProcesses()
    {
        var processes = Process.GetProcesses();

        // ✅ .NET 10 Hot-Path Performance: Array.ConvertAll avoids LINQ heap allocations
        return Array.ConvertAll(processes, static process => (IProcess)new ProcessAdapter(process));
    }

    /// <inheritdoc />
    public bool IsProcessRunning(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var processes = Process.GetProcessesByName(name);

        // ⚠️ Exception Guidelines Section 6: No try/finally required as nothing throws between
        // GetProcessesByName and disposal, and Array.Length remains valid after disposing instances.
        int runningCount = processes.Length;

        foreach (var process in processes)
        {
            process.Dispose();
        }

        return runningCount > 0;
    }

    /// <inheritdoc />
    public void KillProcess(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var processes = Process.GetProcessesByName(name);

        foreach (var process in processes)
        {
            // ⚠️ Exception Guidelines Section 6: Scoped using statement ensures deterministic disposal
            // even when Process.Kill throws (e.g. Win32Exception / Access Denied).
            using (process)
            {
                process.Kill();
            }
        }
    }

    /// <inheritdoc />
    public IProcess? Start(ProcessStartInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);

        var process = Process.Start(info);

        // If the startup was successful, wrap the process instance into the adapter abstraction
        return process is not null ? new ProcessAdapter(process) : null;
    }
}
