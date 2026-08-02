using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Infrastructure.BehavioralComponents.Wrappers;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Wrapper.WindowsOS;

/// <summary>
/// Adapter: Driven Adapter (Outbound Handler/Adapter) implementing process control for the Windows operating system.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Baustein im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core zur Prozesssteuerung (Starten, Stoppen, Fenster-Interaktionen via P/Invoke).<br/>
/// - <strong>Implementierter Port:</strong> <see cref="IOutboundPortOsProcessControl"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein vorbildlicher <strong>Outbound Adapter</strong>, da sie im Infrastructure-Layer liegt, einen Outbound Port implementiert und vom Core angetrieben wird, um OS-Prozess-Seiteneffekte auszuführen.
/// </para>
/// </summary>
[SupportedOSPlatform("windows")]
public sealed partial class AdapterWindowsProcessControlWrapper(
    ILogger<AdapterWindowsProcessControlWrapper> logger,
    IProcessWrapper processWrapper)
    : IOutboundPortOsProcessControl
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════

    // ── Block 2: Primitive Typen & Strings (alphabetisch) ──
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

    // ── Block 1: Injizierte Abhängigkeiten (alphabetisch) ──
    private readonly ILogger<AdapterWindowsProcessControlWrapper> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IProcessWrapper _processWrapper = processWrapper ?? throw new ArgumentNullException(nameof(processWrapper));


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════

    public async Task CloseAllOpenProgramsAsync(int timeoutMilliseconds)
    {
        var processes = _processWrapper.GetProcesses();

        List<Task> pendingTasks = [];
        List<IProcess> processesToDispose = [];

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
            await WaitForProcessesToCloseAsync(pendingTasks, processesToDispose, timeoutMilliseconds).ConfigureAwait(false);
        }
    }

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
            _logger.LogError(exception, "Error terminating {Name}", processName);
        }
    }

    public bool IsProcessAlive(string processName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(processName);
        return _processWrapper.IsProcessRunning(processName);
    }

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

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Explorer opened in: {Path}", folderPath);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error opening Explorer: {Path}", folderPath);
        }
    }

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

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Executable started: {Path} with arguments: {Args}", exeFilePath, arguments);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error starting the EXE: {Path}", exeFilePath);
        }
    }

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
                    _logger.LogWarning("MSI process could not be started (null): {Path}", installerPath);
                }

                return;
            }

            string error = process.StandardError.ReadToEnd();
            string output = process.StandardOutput.ReadToEnd();

            process.WaitForExit();

            if (!string.IsNullOrWhiteSpace(error) && _logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("MSI Installer error output: {Error}", error);
            }

            if (!string.IsNullOrWhiteSpace(output) && _logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("MSI Installer output: {Output}", output);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error starting the MSI installer: {Path}", installerPath);
        }
    }

    public void ShutdownComputer()
    {
        try
        {
            _logger.LogInformation("Shutting down computer (Restart)...");

            string systemFolder = Environment.GetFolderPath(Environment.SpecialFolder.System);
            string shutdownPath = Path.Combine(systemFolder, ShutdownExeFileName);

            _processWrapper.Start(new ProcessStartInfo(shutdownPath, ShutdownRebootForceArguments) { UseShellExecute = true });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error attempting to shut down the computer.");
        }
    }

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

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Executable started: {Path}", exeFilePath);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error starting the EXE: {Path}", exeFilePath);
        }
    }

    public Task StartExecutableAsync(string exeFilePath)
    {
        // ⚡ Immediate Guard-Clause Exception Timing (Guide Abs. 9.1)
        ArgumentException.ThrowIfNullOrWhiteSpace(exeFilePath);

        return StartExecutableCoreAsync(exeFilePath);
    }

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
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Executable started and waiting for termination: {Path}", exeFilePath);
                }

                await process.WaitForExitAsync().ConfigureAwait(false);

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
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Error starting/waiting for the EXE: {exeFilePath}", exception);
        }
    }

    [LibraryImport(User32DllName, EntryPoint = PostMessageWEntryPoint, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    private static bool ShouldIgnoreProcess(IProcess process, string currentProcessName)
    {
        return process.ProcessName == SystemProcessName
               || process.ProcessName == IdleProcessName
               || process.ProcessName.Equals(ExplorerProcessName, StringComparison.OrdinalIgnoreCase)
               || process.ProcessName.Equals(currentProcessName, StringComparison.OrdinalIgnoreCase);
    }

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
            _logger.LogError(exception, "Error sending WM_CLOSE to process {Name}", process.ProcessName);
            process.Dispose();
        }
    }

    private async Task WaitForProcessesToCloseAsync(List<Task> pendingTasks, List<IProcess> processesToDispose, int timeoutMilliseconds)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Waiting for the closure of {Count} programs (Timeout: {Timeout}ms)...", pendingTasks.Count, timeoutMilliseconds);
        }

        try
        {
            await Task.WhenAll(pendingTasks).WaitAsync(TimeSpan.FromMilliseconds(timeoutMilliseconds)).ConfigureAwait(false);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("All programs were successfully closed gracefully.");
            }
        }
        catch (TimeoutException exception)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning(exception, "Timeout reached while waiting for programs to terminate.");
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
