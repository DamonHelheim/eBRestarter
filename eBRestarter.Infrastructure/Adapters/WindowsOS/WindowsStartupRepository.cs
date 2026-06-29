using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace eBRestarter.Infrastructure.Adapters.WindowsOS;

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
public sealed class WindowsStartupRepository(
    ILogger<WindowsStartupRepository> logger,
    ISettingsPort registry,
    IProcessInfoPort processInfo) : IAutoStartPort, IBrowserConfigPort
{
    // Path for "Current User" Run-Key (Standard Autostart for the current user).
    private const string RegistryPathRun = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    // Path for Edge Policies (Machine-wide / HKLM).
    // Controls "Startup Boost" which preloads Edge in the background.
    private const string RegistryPathEdgePolicies = @"SOFTWARE\Policies\Microsoft\Edge";

    // Path for Passwordless Sign-in (Machine-wide / HKLM).
    private const string RegistryPathPasswordLess = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\PasswordLess\Device";

    private readonly ILogger<WindowsStartupRepository> _logger = logger;
    private readonly ISettingsPort _registry = registry;
    private readonly IProcessInfoPort _processInfo = processInfo;

    public void EnableAutoStart()
    {
        try
        {
            string exePath = _processInfo.GetCurrentExecutablePath();
            _registry.SetUserValue(RegistryPathRun, "eBRestarter", exePath);
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
            _registry.DeleteUserValue(RegistryPathRun, "eBRestarter");
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
            return _registry.GetUserValues(RegistryPathRun);
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
    public void SetBrowserStartupBoost(bool enable)
    {
        try
        {
            int dwordValue = enable ? 1 : 0;

            _registry.SetSystemValue(RegistryPathEdgePolicies, "StartupBoostEnabled", dwordValue);

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
    public bool IsBrowserStartupBoostEnabled()
    {
        try
        {
            // Uses the existing registry wrapper
            // Path: SOFTWARE\Policies\Microsoft\Edge
            // Key: StartupBoostEnabled
            var value = _registry.GetSystemValue(RegistryPathEdgePolicies, "StartupBoostEnabled");

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
            _registry.SetSystemValue(RegistryPathPasswordLess, "DevicePasswordLessBuildVersion", dwordValue);

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
        // We simply use your existing, working registry method
        EnableAutoStart();
        return Task.CompletedTask;
    }

    public Task DisableAutoStartAsync()
    {
        // We simply use your existing, working registry method
        DisableAutoStart();
        return Task.CompletedTask;
    }

    public Task<bool> IsAutoStartEnabledAsync()
    {
        try
        {
            // We check if the key "eBRestarter" exists in the registry
            var entries = RetrieveStartupEntries();
            bool isEnabled = entries.ContainsKey("eBRestarter");

            return Task.FromResult(isEnabled);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while checking autostart status.");
            return Task.FromResult(false);
        }
    }
}
