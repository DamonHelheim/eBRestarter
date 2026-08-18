using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using eBRestarter.Core.Application.BehavioralComponents.Extensions;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Infrastructure.BehavioralComponents.Wrappers;
using eBRestarter.Core.Application.ObjectArchetypes.Constants;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Wrapper.WindowsOS;

/// <summary>
/// Adapter: Driven Adapter (Outbound Handler/Adapter) implementing process control for the Windows operating system.
/// <para>
/// <strong>Architecture Classification: OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Role &amp; Responsibility:</strong> Handles process control operations (launch, termination, window interaction via P/Invoke) for Windows in the Infrastructure layer.<br/>
/// - <strong>Implemented Port:</strong> <see cref="IOutboundPortOsProcessControl"/>.<br/>
/// </para>
/// </summary>
/// <param name="logger">Logger instance.</param>
/// <param name="processWrapper">Process wrapper instance.</param>
[SupportedOSPlatform("windows")]
public sealed partial class AdapterWindowsProcessControlWrapper(
    ILogger<AdapterWindowsProcessControlWrapper> logger,
    IProcessWrapper processWrapper)
    : IOutboundPortOsProcessControl
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════

    // ── Block 2: Primitives & strings ──
    private const string ExplorerExeFileName = "explorer.exe";
    private const string ExplorerProcessName = "explorer";
    private const string IdleProcessName = "Idle";
    private const string MsiExecExeFileName = "msiexec.exe";
    private const string MsiInstallArgumentsPattern = "/i \"{0}\"";
    private const string PostMessageWEntryPoint = "PostMessageW";
    private const string ShutdownExeFileName = "shutdown.exe";
    private const string ShutdownRebootForceArguments = "/r /f /t 0";
    private const string SystemProcessName = "System";
    private const string User32DllName = "user32.dll";
    private const uint WmClose = 0x0010;


    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════

    // ── Block 1: Injected dependencies ──
    private readonly ILogger<AdapterWindowsProcessControlWrapper> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IProcessWrapper _processWrapper = processWrapper ?? throw new ArgumentNullException(nameof(processWrapper));


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════

    /// <inheritdoc />
    public async Task CloseAllOpenProgramsAsync(int timeoutMilliseconds)
    {
        var processes = _processWrapper.GetProcesses();

        List<Task> pendingTasks = [];
        List<IProcess> processesToDispose = [];

        // Process handles represent native resources - using declaration ensures proper cleanup.
        using var currentProcess = Process.GetCurrentProcess();
        string currentProcessName = currentProcess.ProcessName;

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
            await WaitForProcessesToCloseAsync(pendingTasks, processesToDispose, timeoutMilliseconds).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public void CloseApplication(string processName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(processName);

        try
        {
            if (_processWrapper.IsProcessRunning(processName))
            {
                _processWrapper.KillProcess(processName);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(LogEventIds.OperatingSystem.ProcessTerminationFailed, exception, "Error terminating {Name}", processName);
        }
    }

    /// <inheritdoc />
    public bool IsProcessAlive(string processName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(processName);
        return _processWrapper.IsProcessRunning(processName);
    }

    /// <inheritdoc />
    public void OpenDirectoryInFileBrowser(string folderPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(folderPath);

        try
        {
            string args = $"\"{folderPath}\"";

            var windowsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            var explorerPath = Path.Combine(windowsFolder, ExplorerExeFileName);

            _processWrapper.Start(new ProcessStartInfo
            {
                FileName = explorerPath,
                Arguments = args,
                UseShellExecute = true
            });

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(LogEventIds.OperatingSystem.ExplorerOpened, "Explorer opened in: {Path}", folderPath);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(LogEventIds.OperatingSystem.ExplorerOpenFailed, exception, "Error opening Explorer: {Path}", folderPath);
        }
    }

    /// <inheritdoc />
    public void OpenUrlInBrowser(string exeFilePath, string arguments)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(exeFilePath);

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = exeFilePath,
                Arguments = arguments,
                UseShellExecute = true
            };

            _processWrapper.Start(startInfo);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                // Mask URL user segment to redact sensitive user credentials in log entries.
                _logger.LogDebug(
                    LogEventIds.OperatingSystem.ProcessStarted,
                    "Executable started: {Path} with arguments: {Args}",
                    exeFilePath,
                    LogRedaction.MaskUrlUserSegment(arguments));
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(LogEventIds.OperatingSystem.ProcessStartFailed, exception, "Error starting the EXE: {Path}", exeFilePath);
        }
    }

    /// <inheritdoc />
    public void RunInstaller(string installerPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(installerPath);

        string systemFolder = Environment.GetFolderPath(Environment.SpecialFolder.System);
        string msiExecPath = Path.Combine(systemFolder, MsiExecExeFileName);

        var startInfo = new ProcessStartInfo
        {
            FileName = msiExecPath,
            Arguments = string.Format(MsiInstallArgumentsPattern, installerPath),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        try
        {
            using var process = _processWrapper.Start(startInfo);

            if (process is null)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning(LogEventIds.OperatingSystem.InstallerNotStarted, "MSI process could not be started (null): {Path}", installerPath);
                }

                return;
            }

            string error = process.StandardError.ReadToEnd();
            string output = process.StandardOutput.ReadToEnd();

            process.WaitForExit();

            if (!string.IsNullOrWhiteSpace(error) && _logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning(LogEventIds.OperatingSystem.InstallerOutputReceived, "MSI Installer error output: {Error}", error);
            }

            if (!string.IsNullOrWhiteSpace(output) && _logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(LogEventIds.OperatingSystem.InstallerOutputReceived, "MSI Installer output: {Output}", output);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(LogEventIds.OperatingSystem.InstallerStartFailed, exception, "Error starting the MSI installer: {Path}", installerPath);
        }
    }

    /// <inheritdoc />
    public void ShutdownComputer()
    {
        try
        {
            _logger.LogInformation(LogEventIds.OperatingSystem.ProcessShutdownRequested, "Shutting down computer (Restart)...");

            string systemFolder = Environment.GetFolderPath(Environment.SpecialFolder.System);
            string shutdownPath = Path.Combine(systemFolder, ShutdownExeFileName);

            _processWrapper.Start(new ProcessStartInfo(shutdownPath, ShutdownRebootForceArguments) { UseShellExecute = true });
        }
        catch (Exception exception)
        {
            _logger.LogError(LogEventIds.OperatingSystem.ProcessShutdownFailed, exception, "Error attempting to shut down the computer.");
        }
    }

    /// <inheritdoc />
    public void StartExecutable(string exeFilePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(exeFilePath);

        try
        {
            _processWrapper.Start(new ProcessStartInfo
            {
                FileName = exeFilePath,
                UseShellExecute = true
            });

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(LogEventIds.OperatingSystem.ProcessStarted, "Executable started: {Path}", exeFilePath);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(LogEventIds.OperatingSystem.ProcessStartFailed, exception, "Error starting the EXE: {Path}", exeFilePath);
        }
    }

    /// <inheritdoc />
    public Task StartExecutableAsync(string exeFilePath)
    {
        // Immediate guard clause validation prior to async state machine instantiation.
        ArgumentException.ThrowIfNullOrWhiteSpace(exeFilePath);

        return StartExecutableCoreAsync(exeFilePath);
    }

    /// <summary>
    /// Asynchronously starts an executable and waits for process termination.
    /// </summary>
    /// <param name="exeFilePath">Target executable file path.</param>
    private async Task StartExecutableCoreAsync(string exeFilePath)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = exeFilePath,
                UseShellExecute = true
            };

            using var process = _processWrapper.Start(startInfo);

            if (process is not null)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug(LogEventIds.OperatingSystem.ProcessStarted, "Executable started and waiting for termination: {Path}", exeFilePath);
                }

                await process.WaitForExitAsync().ConfigureAwait(false);

                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug(LogEventIds.OperatingSystem.ProcessTerminated, "Executable was terminated: {Path}", exeFilePath);
                }
            }
            else
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning(LogEventIds.OperatingSystem.InstallerNotStarted, "Process could not be started (received null): {Path}", exeFilePath);
                }
            }
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Error starting/waiting for the EXE: {exeFilePath}", exception);
        }
    }

    /// <summary>
    /// Posts a Windows message to the message queue of the specified window.
    /// </summary>
    [LibraryImport(User32DllName, EntryPoint = PostMessageWEntryPoint, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    /// <summary>
    /// Determines whether the specified process should be excluded from graceful closure.
    /// </summary>
    /// <param name="process">Process instance.</param>
    /// <param name="currentProcessName">Name of the executing application process.</param>
    private static bool ShouldIgnoreProcess(IProcess process, string currentProcessName)
    {
        return process.ProcessName == SystemProcessName
               || process.ProcessName == IdleProcessName
               || process.ProcessName.Equals(ExplorerProcessName, StringComparison.OrdinalIgnoreCase)
               || process.ProcessName.Equals(currentProcessName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Attempts to send WM_CLOSE to a process window or disposes handle if windowless.
    /// </summary>
    /// <param name="process">Process instance.</param>
    /// <param name="pendingTasks">List of pending termination tasks.</param>
    /// <param name="processesToDispose">List of processes to dispose post-completion.</param>
    private void TryCloseProcess(IProcess process, List<Task> pendingTasks, List<IProcess> processesToDispose)
    {
        try
        {
            if (process.MainWindowHandle != IntPtr.Zero)
            {
                PostMessage(process.MainWindowHandle, WmClose, IntPtr.Zero, IntPtr.Zero);
                pendingTasks.Add(process.WaitForExitAsync());
                processesToDispose.Add(process);
            }
            else
            {
                process.Dispose();
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(LogEventIds.OperatingSystem.ProcessCloseRequestFailed, exception, "Error sending WM_CLOSE to process {Name}", process.ProcessName);
            process.Dispose();
        }
    }

    /// <summary>
    /// Waits for pending process close tasks to complete within specified timeout.
    /// </summary>
    /// <param name="pendingTasks">List of pending termination tasks.</param>
    /// <param name="processesToDispose">List of processes to dispose post-completion.</param>
    /// <param name="timeoutMilliseconds">Timeout in milliseconds.</param>
    private async Task WaitForProcessesToCloseAsync(List<Task> pendingTasks, List<IProcess> processesToDispose, int timeoutMilliseconds)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(LogEventIds.OperatingSystem.ProcessCloseRequestFailed, "Waiting for the closure of {Count} programs (Timeout: {Timeout}ms)...", pendingTasks.Count, timeoutMilliseconds);
        }

        try
        {
            await Task.WhenAll(pendingTasks).WaitAsync(TimeSpan.FromMilliseconds(timeoutMilliseconds)).ConfigureAwait(false);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(LogEventIds.OperatingSystem.ProcessesClosedGracefully, "All programs were successfully closed gracefully.");
            }
        }
        catch (TimeoutException exception)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning(LogEventIds.OperatingSystem.ProcessCloseTimeout, exception, "Timeout reached while waiting for programs to terminate.");
            }
        }
        finally
        {
            foreach (var process in processesToDispose)
            {
                process.Dispose();
            }
        }
    }
}
