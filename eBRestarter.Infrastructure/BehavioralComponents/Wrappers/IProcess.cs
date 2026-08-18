using System;
using System.IO;
using System.Threading.Tasks;

namespace eBRestarter.Infrastructure.BehavioralComponents.Wrappers;

/// <summary>
/// Abstraction interface representing an operating system process instance for testability and lifecycle management.
/// </summary>
public interface IProcess : IDisposable
{
    /// <summary>
    /// Gets a stream used to read the textual output of the application.
    /// </summary>
    StreamReader StandardOutput { get; }

    /// <summary>
    /// Gets a stream used to read the error output of the application.
    /// </summary>
    StreamReader StandardError { get; }

    /// <summary>
    /// Gets the name of the process.
    /// </summary>
    string ProcessName { get; }

    /// <summary>
    /// Gets the window handle of the main window of the associated process.
    /// </summary>
    IntPtr MainWindowHandle { get; }

    /// <summary>
    /// Instructs the process component to wait indefinitely for the associated process to exit.
    /// </summary>
    void WaitForExit();

    /// <summary>
    /// Instructs the process component to wait up to the specified time for the associated process to exit.
    /// </summary>
    /// <param name="milliseconds">The amount of time, in milliseconds, to wait for the associated process to exit.</param>
    /// <returns><see langword="true"/> if the associated process has exited; otherwise, <see langword="false"/>.</returns>
    bool WaitForExit(int milliseconds);

    /// <summary>
    /// Asynchronously waits for the associated process to exit.
    /// </summary>
    /// <returns>A task that represents the asynchronous wait operation.</returns>
    Task WaitForExitAsync();
}
