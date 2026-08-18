using System.Diagnostics;

namespace eBRestarter.Infrastructure.BehavioralComponents.Wrappers;

/// <summary>
/// Abstraction interface providing system process management operations (start, query, termination, and enumeration).
/// </summary>
public interface IProcessWrapper
{
    /// <summary>
    /// Starts a process resource specified by the <see cref="ProcessStartInfo"/> configuration.
    /// </summary>
    /// <param name="info">The <see cref="ProcessStartInfo"/> containing startup parameters.</param>
    /// <returns>An <see cref="IProcess"/> abstraction wrapping the started process, or <see langword="null"/> if no process was started.</returns>
    IProcess? Start(ProcessStartInfo info);

    /// <summary>
    /// Checks whether at least one instance of a process with the specified name is currently running.
    /// </summary>
    /// <param name="name">The friendly name of the process (without file extension).</param>
    /// <returns><see langword="true"/> if at least one matching process instance is running; otherwise, <see langword="false"/>.</returns>
    bool IsProcessRunning(string name);

    /// <summary>
    /// Terminates all running instances of the specified process immediately.
    /// </summary>
    /// <param name="name">The friendly name of the process to terminate.</param>
    void KillProcess(string name);

    /// <summary>
    /// Retrieves all running processes wrapped in testable <see cref="IProcess"/> instances.
    /// </summary>
    /// <returns>An array of <see cref="IProcess"/> instances representing all active processes.</returns>
    IProcess[] GetProcesses();
}

