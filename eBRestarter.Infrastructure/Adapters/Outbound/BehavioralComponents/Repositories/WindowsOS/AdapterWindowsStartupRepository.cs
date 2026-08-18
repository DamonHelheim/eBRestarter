using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.ObjectArchetypes.Constants;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Repositories.WindowsOS;

/// <summary>
/// Adapter: Driven Adapter (Outbound Repository) managing system startup configurations in Windows Registry.
/// <para>
/// <strong>Architecture Classification: OUTBOUND ADAPTER (Driven Adapter / Repository)</strong><br/>
/// - <strong>Role &amp; Responsibility:</strong> Configures Windows Autostart, Edge Startup Boost, and AutoLogon via Registry in the Infrastructure layer.<br/>
/// - <strong>Implemented Ports:</strong> <see cref="IOutboundPortAutoStartRepository"/> and <see cref="IOutboundPortBrowserConfigRepository"/>.<br/>
/// </para>
/// </summary>
public sealed class AdapterWindowsStartupRepository : IOutboundPortAutoStartRepository, IOutboundPortBrowserConfigRepository
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitives & strings ──
    private const string AccessDeniedEdgePolicyExceptionMessage = "Access denied when changing registry settings. Please run as Administrator.";
    private const string AccessDeniedPasswordLessExceptionMessage = "Access denied when changing PasswordLess settings. Please run as Administrator.";
    private const int ActiveDwordValue = 1;
    private const string ActiveStateText = "Active";
    private const string ApplicationAutostartKey = "eBRestarter";
    private const string AutoLogonSettingLogMessage = "AutoLogon setting set to {State} (RegValue: {Value}).";
    private const string AutostartCreatedLogMessage = "Autostart entry for 'eBRestarter' successfully created.";
    private const string AutostartRemovedLogMessage = "Autostart entry for 'eBRestarter' removed.";
    private const string CouldNotReadEdgePolicyLogMessage = "Could not read Edge Startup Boost status.";
    private const string DevicePasswordLessBuildVersionValueName = "DevicePasswordLessBuildVersion";
    private const int DisabledDwordValue = 0;
    private const string EdgeStartupBoostEnabledValueName = "StartupBoostEnabled";
    private const string EdgeStartupBoostLogMessage = "Edge Startup Boost set to {State} (Value: {Value}).";
    private const string ErrorCheckingAutostartStatusLogMessage = "Error while checking autostart status.";
    private const string ErrorRemovingAutostartLogMessage = "Error removing autostart from registry.";
    private const string ErrorRetrievingAutostartEntriesLogMessage = "Error retrieving autostart entries.";
    private const string ErrorSettingAutostartLogMessage = "Error setting autostart in registry.";
    private const string FailedEdgePolicyExceptionMessage = "Failed to set Edge Startup Boost policy.";
    private const string FailedPasswordLessExceptionMessage = "Failed to configure PasswordLess AutoLogon registry setting.";
    private const int InactiveDwordValue = 2;
    private const string InactiveStateText = "Inactive";
    private const string RegistryPathEdgePolicies = @"SOFTWARE\Policies\Microsoft\Edge";
    private const string RegistryPathPasswordLess = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\PasswordLess\Device";
    private const string RegistryPathRun = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injected dependencies ──
    private readonly ILogger<AdapterWindowsStartupRepository> _logger;
    private readonly IOutboundPortProcessInfoProvider _processInfo;
    private readonly IOutboundPortSystemConfigurationRepository _registry;


    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Initializes a new instance of <see cref="AdapterWindowsStartupRepository"/>.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="processInfo">Process information provider port.</param>
    /// <param name="registry">System configuration repository port.</param>
    public AdapterWindowsStartupRepository(
        ILogger<AdapterWindowsStartupRepository> logger,
        IOutboundPortProcessInfoProvider processInfo,
        IOutboundPortSystemConfigurationRepository registry)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(processInfo);
        ArgumentNullException.ThrowIfNull(registry);

        _logger = logger;
        _processInfo = processInfo;
        _registry = registry;
    }


    // ═══════════════════════════════════════════════════════
    //  8. Methods (public → private)
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Removes the application autostart entry from the Windows Registry Run key.
    /// </summary>
    public void DisableAutoStart()
    {
        try
        {
            _registry.DeleteUserValue(RegistryPathRun, ApplicationAutostartKey);
            _logger.LogInformation(LogEventIds.OperatingSystem.AutoStartDisabled, AutostartRemovedLogMessage);
        }
        catch (Exception exception)
        {
            // Exception handling: Wrap and rethrow exception without logging locally to prevent duplicate log entries.
            throw new InvalidOperationException(ErrorRemovingAutostartLogMessage, exception);
        }
    }

    /// <summary>
    /// Asynchronously removes the application autostart entry from the Windows Registry Run key.
    /// </summary>
    public Task DisableAutoStartAsync()
    {
        DisableAutoStart();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Creates or updates the application autostart entry in the Windows Registry Run key.
    /// </summary>
    /// <remarks>
    /// Security: Encloses executable path in quotes to prevent unquoted path vulnerability hijacking when paths contain spaces.
    /// </remarks>
    public void EnableAutoStart()
    {
        try
        {
            string exePath = _processInfo.GetCurrentExecutablePath();
            _registry.SetUserValue(RegistryPathRun, ApplicationAutostartKey, $"\"{exePath}\"");
            _logger.LogInformation(LogEventIds.OperatingSystem.AutoStartEnabled, AutostartCreatedLogMessage);
        }
        catch (Exception exception)
        {
            // Exception handling: Wrap and rethrow exception without logging locally to prevent duplicate log entries.
            throw new InvalidOperationException(ErrorSettingAutostartLogMessage, exception);
        }
    }

    /// <summary>
    /// Asynchronously creates or updates the application autostart entry in the Windows Registry Run key.
    /// </summary>
    public Task EnableAutoStartAsync()
    {
        EnableAutoStart();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously checks whether an autostart entry for this application exists in the Registry.
    /// </summary>
    public Task<bool> IsAutoStartEnabledAsync()
    {
        try
        {
            var entries = RetrieveStartupEntries();
            bool isEnabled = entries.ContainsKey(ApplicationAutostartKey);
            return Task.FromResult(isEnabled);
        }
        catch (Exception exception)
        {
            _logger.LogError(LogEventIds.OperatingSystem.AutoStartStatusReadFailed, exception, ErrorCheckingAutostartStatusLogMessage);
            return Task.FromResult(false);
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
            var value = _registry.GetSystemValue(RegistryPathEdgePolicies, EdgeStartupBoostEnabledValueName);

            if (value is ActiveDwordValue)
            {
                return true;
            }

            if (value is DisabledDwordValue)
            {
                return false;
            }
        }
        catch (Exception exception)
        {
            _logger.LogWarning(LogEventIds.OperatingSystem.EdgeStartupBoostReadFailed, exception, CouldNotReadEdgePolicyLogMessage);
        }

        return true;
    }

    /// <summary>
    /// Retrieves all startup entries present in the Windows Registry Run key.
    /// </summary>
    public Dictionary<string, object> RetrieveStartupEntries()
    {
        try
        {
            return _registry.GetUserValues(RegistryPathRun);
        }
        catch (Exception exception)
        {
            _logger.LogError(LogEventIds.OperatingSystem.AutoStartStatusReadFailed, exception, ErrorRetrievingAutostartEntriesLogMessage);
            return [];
        }
    }

    /// <summary>
    /// Configures the AutoLogon passwordless registry toggle.
    /// </summary>
    /// <param name="enable">True to enable AutoLogon; false otherwise.</param>
    public void SetAutoLogon(bool enable)
    {
        try
        {
            // Logic inverted: Enable AutoLogon means PasswordLess Disabled (0)
            int dwordValue = enable ? DisabledDwordValue : InactiveDwordValue;
            _registry.SetSystemValue(RegistryPathPasswordLess, DevicePasswordLessBuildVersionValueName, dwordValue);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(LogEventIds.Security.PasswordlessModeChanged, AutoLogonSettingLogMessage, enable ? ActiveStateText : InactiveStateText, dwordValue);
            }
        }
        catch (UnauthorizedAccessException exception)
        {
            throw new UnauthorizedAccessException(AccessDeniedPasswordLessExceptionMessage, exception);
        }
        catch (Exception exception)
        {
            // Exception handling: Wrap and rethrow exception without logging locally to prevent duplicate log entries.
            throw new InvalidOperationException(FailedPasswordLessExceptionMessage, exception);
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
            int dwordValue = enable ? ActiveDwordValue : DisabledDwordValue;

            _registry.SetSystemValue(RegistryPathEdgePolicies, EdgeStartupBoostEnabledValueName, dwordValue);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(LogEventIds.OperatingSystem.EdgeStartupBoostChanged, EdgeStartupBoostLogMessage, enable ? ActiveStateText : InactiveStateText, dwordValue);
            }
        }
        catch (UnauthorizedAccessException exception)
        {
            throw new UnauthorizedAccessException(AccessDeniedEdgePolicyExceptionMessage, exception);
        }
        catch (Exception exception)
        {
            // Exception handling: Wrap and rethrow exception without logging locally to prevent duplicate log entries.
            throw new InvalidOperationException(FailedEdgePolicyExceptionMessage, exception);
        }
    }
}
