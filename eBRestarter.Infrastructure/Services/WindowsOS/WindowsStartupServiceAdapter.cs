using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS.Process;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace eBRestarter.Infrastructure.Services.WindowsOS;

/// <summary>
/// Manages system startup configurations, including Windows Autostart, Edge Startup Boost
/// and AutoLogon settings via the Windows Registry.
/// <br/>
/// <b>Architecture Layer:</b> Infrastructure (Adapter)
/// <br/>
/// <b>Responsibility:</b> Encapsulates logic for reading and writing registry values
/// to influence Windows startup behavior. Uses wrapper interfaces
/// to ensure testability (mocking) of static registry classes.
/// </summary>
public class WindowsStartupServiceAdapter(
    ILogger<WindowsStartupServiceAdapter> logger,
    IWindowsRegistryService registry,
    IProcessInfoUseCase processInfo) : IWindowsStartupManagerService
{
    // ID must match exactly with the one in Package.appxmanifest! (Nur noch relevant zur Doku, wird in Unpackaged nicht mehr für StartupTask genutzt)
#pragma warning disable RCS1213 // Remove unused member declaration
    private const string StartupTaskId = "eBRestarterAutoStart";
#pragma warning restore RCS1213 // Remove unused member declaration

    // Path for "Current User" Run-Key (Standard Autostart for the current user).
    private const string RegistryPathRun = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    // Path for Edge Policies (Machine-wide / HKLM).
    // Controls "Startup Boost" which preloads Edge in the background.
    private const string RegistryPathEdgePolicies = @"SOFTWARE\Policies\Microsoft\Edge";

    // Path for Passwordless Sign-in (Machine-wide / HKLM).
    private const string RegistryPathPasswordLess = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\PasswordLess\Device";

    private readonly ILogger<WindowsStartupServiceAdapter> _logger = logger;
    private readonly IWindowsRegistryService _registry = registry;
    private readonly IProcessInfoUseCase _processInfo = processInfo;

    public void EnableAutoStart()
    {
        try
        {
            string exePath = _processInfo.GetCurrentExecutablePath();
            _registry.SetCurrentUserValue(RegistryPathRun, "eBRestarter", exePath);
            _logger.LogInformation("Autostart entry for 'eBRestarter' successfully created.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting autostart in registry.");
        }
    }

    public void DisableAutoStart()
    {
        try
        {
            _registry.DeleteCurrentUserValue(RegistryPathRun, "eBRestarter");
            _logger.LogInformation("Autostart entry for 'eBRestarter' removed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing autostart from registry.");
        }
    }

    public Dictionary<string, object> RetrieveStartupEntries()
    {
        try
        {
            return _registry.RetrieveCurrentUserValues(RegistryPathRun);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving autostart entries.");

            return [];
        }
    }

    /// <summary>
    /// Activates or deactivates the "Startup Boost" feature of Microsoft Edge.
    /// </summary>
    /// <param name="enable">
    /// <c>true</c>: Sets registry value to 1 (Enabled).
    /// <c>false</c>: Sets registry value to 0 (Disabled).
    /// </param>
    public void SetEdgeStartupBoost(bool enable)
    {
        try
        {
            int dwordValue = enable ? 1 : 0;

            _registry.SetLocalMachineValue(RegistryPathEdgePolicies, "StartupBoostEnabled", dwordValue, RegistryValueKind.DWord);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Edge Startup Boost set to {State} (Value: {Value}).", enable, dwordValue);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Access denied when changing Edge policies. Please run as Administrator.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "General error setting Edge Startup Boost.");
        }
    }

    /// <summary>
    /// Checks if Edge Startup Boost is currently enabled via registry policy.
    /// </summary>
    /// <returns>True if enabled (1), False if disabled (0). Returns true (default) if not set.</returns>
    public bool IsEdgeStartupBoostEnabled()
    {
        try
        {
            // Uses the existing registry wrapper
            // Path: SOFTWARE\Policies\Microsoft\Edge
            // Key: StartupBoostEnabled
            var value = _registry.RetrieveLocalMachineValue(RegistryPathEdgePolicies, "StartupBoostEnabled");

            if (value is int intValue)
            {
                // 1 = Enabled, 0 = Disabled
                return intValue == 1;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not read Edge Startup Boost status.");
        }

        // Default assumption: If no policy is set, user setting applies (often on by default).
        // For policy control "Not Present" means "Not Managed".
        // Returning true as a safe default or purely based on policy presence logic.
        return true;
    }

    public void SetAutoLogon(bool enable)
    {
        try
        {
            // Logic inverted: Enable AutoLogon means PasswordLess Disabled (0)
            int dwordValue = enable ? 0 : 2;
            _registry.SetLocalMachineValue(RegistryPathPasswordLess, "DevicePasswordLessBuildVersion", dwordValue, RegistryValueKind.DWord);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("AutoLogon setting set to {State} (RegValue: {Value}).", enable, dwordValue);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Access denied when changing PasswordLess settings. Please run as Administrator.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring AutoLogon settings.");
        }
    }
    // PART 1: Autostart via Registry (Unpackaged)

    public Task EnableAutoStartAsync()
    {
        // Wir nutzen einfach deine bereits funktionierende Registry-Methode
        EnableAutoStart();
        return Task.CompletedTask;
    }

    public Task DisableAutoStartAsync()
    {
        // Wir nutzen einfach deine bereits funktionierende Registry-Methode
        DisableAutoStart();
        return Task.CompletedTask;
    }

    public Task<bool> IsAutoStartEnabledAsync()
    {
        try
        {
            // Wir prüfen, ob der Schlüssel "eBRestarter" in der Registry existiert
            var entries = RetrieveStartupEntries();
            bool isEnabled = entries.ContainsKey("eBRestarter");

            return Task.FromResult(isEnabled);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Prüfen des Autostart-Status.");
            return Task.FromResult(false);
        }
    }
}

