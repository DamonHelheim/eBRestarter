using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Infrastructure.Adapters.Wrapper;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace eBRestarter.Infrastructure.Adapters.WindowsOS;

/// <summary>
/// Implements the <see cref="IProcessControlService"/> for the Windows operating system.
/// <br/>
/// <b>Architecture Layer:</b> Infrastructure (Adapter)
/// <br/>
/// <b>Responsibility:</b> Encapsulates the technical implementation of process control.
/// Uses an <see cref="IProcessWrapper"/> to make system calls testable,
/// and P/Invoke for window interactions.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="WindowsProcessControlAdapter"/>.
/// </remarks>
/// <param name="logger">The logger for error and informational messages.</param>
/// <param name="processWrapper">The wrapper for system process calls (Injected).</param>
[SupportedOSPlatform("windows")]
public sealed partial class WindowsProcessControlAdapter(ILogger<WindowsProcessControlAdapter> logger, IProcessWrapper processWrapper) : IOsProcessControlOutboundPort
{
    private readonly ILogger<WindowsProcessControlAdapter> _logger = logger;

    /// <summary>
    /// Abstraction layer for <see cref="Process"/> calls to enable unit testing.
    /// </summary>
    private readonly IProcessWrapper _processWrapper = processWrapper;

    /// <summary>
    /// Starts an external application (.exe).
    /// </summary>
    /// <param name="exeFilePath">The full path to the executable file.</param>
    /// <remarks>
    /// Sets <c>UseShellExecute = true</c> so that Windows treats the file as if
    /// the user double-clicked it in Explorer (takes into account UAC, paths, and associations).
    /// </remarks>
    public void StartExecutable(string exeFilePath)
    {
        try
        {
            _processWrapper.Start(new ProcessStartInfo
            {
                FileName = exeFilePath,
                UseShellExecute = true
            });

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Executable started: {Path}", exeFilePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting the EXE: {Path}", exeFilePath);
        }
    }

    /// <summary>
    /// Starts an MSI installer (via msiexec.exe) and logs its text output.
    /// </summary>
    /// <param name="msiFilePath">The path to the .msi file.</param>
    /// <remarks>
    /// <b>Warning:</b> This method runs <i>synchronously</i> and blocks the calling thread
    /// until the installation is complete in order to fully read the logs (StdOut/StdErr).
    /// </remarks>
    public void RunInstaller(string msiFilePath)
    {
        // SonarQube Fix: Get the absolute path to msiexec.exe from the System32 folder
        // to prevent path hijacking via environment variables.
        string systemFolder = Environment.GetFolderPath(Environment.SpecialFolder.System);
        string msiExecPath = Path.Combine(systemFolder, "msiexec.exe");

        var startInfo = new ProcessStartInfo
        {
            FileName = msiExecPath, // <-- We are now using the absolutely secure path here!
            Arguments = $"/i \"{msiFilePath}\"",

            // UseShellExecute = false is strictly required to redirect output streams.
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,

            // Suppresses the popping up of an empty console window.
            CreateNoWindow = true
        };

        try
        {
            // Starts the process via the wrapper
            using var process = _processWrapper.Start(startInfo);

            if (process == null)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning("MSI process could not be started (null): {Path}", msiFilePath);
                }
                return;
            }

            // Reads the output streams (blocking until the end)
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            // Logs errors or outputs, if present
            if (!string.IsNullOrWhiteSpace(error) && _logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("MSI Installer error output: {Error}", error);
            }

            if (!string.IsNullOrWhiteSpace(output) && _logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("MSI Installer output: {Output}", output);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting the MSI installer: {Path}", msiFilePath);
        }
    }

    /// <summary>
    /// Starts an external application (.exe) with optional arguments.
    /// </summary>
    /// <param name="exeFilePath">The full path to the executable file.</param>
    /// <param name="arguments">Optional arguments (e.g. a URL).</param>
    public void OpenUrlInBrowser(string exeFilePath, string arguments)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = exeFilePath,
                Arguments = arguments, // HERE: The argument (the URL) is set
                UseShellExecute = true
            };

            _processWrapper.Start(startInfo);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Executable started: {Path} with arguments: {Args}", exeFilePath, arguments);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting the EXE: {Path}", exeFilePath);
        }
    }

    /// <summary>
    /// Forces an immediate restart of the computer.
    /// </summary>
    /// <remarks>
    /// Calls <c>shutdown.exe</c> with the parameters <c>/r</c> (Reboot), <c>/f</c> (Force Close) and <c>/t 0</c> (Immediate).
    /// </remarks>
    public void ShutdownComputer()
    {
        try
        {
            _logger.LogInformation("Shutting down computer (Restart)...");

            // SonarQube Fix: Get the absolute path to shutdown.exe from the System32 folder
            string systemFolder = Environment.GetFolderPath(Environment.SpecialFolder.System);
            string shutdownPath = Path.Combine(systemFolder, "shutdown.exe");

            _processWrapper.Start(new ProcessStartInfo(shutdownPath, "/r /f /t 0") { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error attempting to shut down the computer.");
        }
    }

    /// <summary>
    /// Checks if at least one instance of a process with the specified name is running.
    /// </summary>
    /// <param name="processName">The name of the process (without .exe).</param>
    /// <returns><c>true</c> if the process is running, otherwise <c>false</c>.</returns>
    public bool IsProcessAlive(string processName)
    {
        return _processWrapper.IsProcessRunning(processName);
    }

    /// <summary>
    /// Forcefully terminates all instances of an application (Kill), if they are running.
    /// </summary>
    /// <param name="processName">The name of the process to be terminated.</param>
    public void CloseApplication(string processName)
    {
        try
        {
            if (_processWrapper.IsProcessRunning(processName))
            {
                // KillProcess in the wrapper executes process.Kill().
                _processWrapper.KillProcess(processName);
            }
        }
        catch (Exception ex)
        {
            // Catches errors, e.g. if the process has system privileges and we are not allowed to terminate it.
            _logger.LogError(ex, "Error terminating {Name}", processName);
        }
    }

    /// <summary>
    /// Attempts to gracefully close all visible desktop programs.
    /// </summary>
    /// <remarks>
    /// This method sends a <c>WM_CLOSE</c> message to the main window of each process
    /// (equivalent to clicking the X). Waits asynchronously for the termination.
    /// <br/>
    /// Critical system processes ("System", "Idle") are ignored.
    /// </remarks>
    public async Task CloseAllOpenProgramsAsync(int timeoutMilliseconds)
    {
        var processes = _processWrapper.GetProcesses();
        var pendingTasks = new List<Task>();
        var processesToDispose = new List<IProcess>();

        // SOLUTION: Determine own process name to prevent self-termination!
        string currentProcessName = Process.GetCurrentProcess().ProcessName;

        foreach (var process in processes)
        {
            if (ShouldIgnoreProcess(process, currentProcessName))
            {
                process.Dispose();
                continue;
            }

            TryCloseProcess(process, pendingTasks, processesToDispose);
        }

        if (pendingTasks.Count > 0)
        {
            await WaitForProcessesToCloseAsync(pendingTasks, processesToDispose, timeoutMilliseconds);
        }
    }

    private static bool ShouldIgnoreProcess(IProcess process, string currentProcessName)
    {
        return process.ProcessName == "System"
                     || process.ProcessName == "Idle"
                     || process.ProcessName.Equals("explorer", StringComparison.OrdinalIgnoreCase)
                     || process.ProcessName.Equals(currentProcessName, StringComparison.OrdinalIgnoreCase);
    }

    private void TryCloseProcess(IProcess process, List<Task> pendingTasks, List<IProcess> processesToDispose)
    {
        try
        {
            if (process.MainWindowHandle != IntPtr.Zero)
            {
                PostMessage(process.MainWindowHandle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                pendingTasks.Add(process.WaitForExitAsync());
                processesToDispose.Add(process);
            }
            else
            {
                process.Dispose();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending WM_CLOSE to process {Name}", process.ProcessName);
            process.Dispose();
        }
    }

    private async Task WaitForProcessesToCloseAsync(List<Task> pendingTasks, List<IProcess> processesToDispose, int timeoutMilliseconds)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Waiting for the closure of {Count} programs (Timeout: {Timeout}ms)...", pendingTasks.Count, timeoutMilliseconds);
        }

        var allTasksFinished = Task.WhenAll(pendingTasks);
        var timeoutTask = Task.Delay(timeoutMilliseconds);

        var finishedTask = await Task.WhenAny(allTasksFinished, timeoutTask);

        if (finishedTask == timeoutTask)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("Timeout reached while waiting for programs to terminate.");
            }
        }
        else
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("All programs were successfully closed gracefully.");
            }
        }

        foreach (var process in processesToDispose)
        {
            process.Dispose();
        }
    }

    /// <summary>
    /// Imports the <c>PostMessage</c> function from <c>user32.dll</c>.
    /// Allows sending messages to window handles.
    /// </summary>
    [LibraryImport("user32.dll", EntryPoint = "PostMessageW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    public async Task StartExecutableAsync(string exeFilePath)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = exeFilePath,
                UseShellExecute = true
            };

            // 1. Start process
            // Important: We use 'using' so that resources are cleaned up,
            // BUT only after we have waited.
            using var process = _processWrapper.Start(startInfo);

            if (process != null)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Executable started and waiting for termination: {Path}", exeFilePath);
                }

                // 2. Wait asynchronously
                // This does NOT technically block the UI thread, but the method waits logically here.
                await process.WaitForExitAsync();

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Executable was terminated: {Path}", exeFilePath);
                }
            }
            else
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning("Process could not be started (received null): {Path}", exeFilePath);
                }
            }
        }
        catch (Exception ex)
        {
            // S2139 Fix: Do not log AND throw exception, but rethrow with context
            throw new InvalidOperationException($"Error starting/waiting for the EXE: {exeFilePath}", ex);
        }
    }

    public void OpenDirectoryInFileBrowser(string folderPath)
    {
        try
        {
            // As a precaution, wrap quotes around the path if it contains spaces.
            string args = $"\"{folderPath}\"";

            // SonarQube Fix: Get the absolute path to explorer.exe from the main Windows folder
            var windowsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            var explorerPath = Path.Combine(windowsFolder, "explorer.exe");

            _processWrapper.Start(new ProcessStartInfo
            {
                FileName = explorerPath,
                Arguments = args,
                UseShellExecute = true // Important for Explorer interaction
            });

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Explorer opened in: {Path}", folderPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening Explorer: {Path}", folderPath);
        }
    }

    /// <summary>
    /// Windows Message ID for "Close Window" (0x0010).
    /// </summary>
    private const uint WM_CLOSE = 0x0010;
}
