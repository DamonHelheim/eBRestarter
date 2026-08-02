using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Repositories.WindowsOS;

/// <summary>
/// Adapter: Driven Adapter (Outbound Repository) managing system startup configurations in Windows Registry.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter / Repository)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Baustein im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core zur Konfiguration von Windows Autostart, Edge Startup Boost und AutoLogon via Registry.<br/>
/// - <strong>Implementierte Ports:</strong> <see cref="IOutboundPortAutoStartRepository"/> und <see cref="IOutboundPortBrowserConfigRepository"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein vorbildlicher <strong>Outbound Adapter</strong>, da sie im Infrastructure-Layer liegt, Outbound Ports implementiert und vom Core angetrieben wird, um Systemstart-Einstellungen zu steuern.
/// </para>
/// </summary>
public sealed class AdapterWindowsStartupRepository : IOutboundPortAutoStartRepository, IOutboundPortBrowserConfigRepository
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitive Typen & Strings ──
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
    private const string ErrorConfiguringAutoLogonLogMessage = "Error configuring AutoLogon settings.";
    private const string ErrorRemovingAutostartLogMessage = "Error removing autostart from registry.";
    private const string ErrorRetrievingAutostartEntriesLogMessage = "Error retrieving autostart entries.";
    private const string ErrorSettingAutostartLogMessage = "Error setting autostart in registry.";
    private const string FailedEdgePolicyExceptionMessage = "Failed to set Edge Startup Boost policy.";
    private const string FailedPasswordLessExceptionMessage = "Failed to configure PasswordLess AutoLogon registry setting.";
    private const string GeneralErrorEdgePolicyLogMessage = "General error setting Edge Startup Boost.";
    private const int InactiveDwordValue = 2;
    private const string InactiveStateText = "Inactive";
    private const string RegistryPathEdgePolicies = @"SOFTWARE\Policies\Microsoft\Edge";
    private const string RegistryPathPasswordLess = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\PasswordLess\Device";
    private const string RegistryPathRun = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injizierte Abhängigkeiten (Dependencies) ──
    private readonly ILogger<AdapterWindowsStartupRepository> _logger;
    private readonly IOutboundPortProcessInfoProvider _processInfo;
    private readonly IOutboundPortSystemConfigurationRepository _registry;


    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════
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
    public void DisableAutoStart()
    {
        try
        {
            _registry.DeleteUserValue(RegistryPathRun, ApplicationAutostartKey);
            _logger.LogInformation(AutostartRemovedLogMessage);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, ErrorRemovingAutostartLogMessage);
            throw new InvalidOperationException(ErrorRemovingAutostartLogMessage, exception);
        }
    }

    public Task DisableAutoStartAsync()
    {
        DisableAutoStart();
        return Task.CompletedTask;
    }

    public void EnableAutoStart()
    {
        try
        {
            string exePath = _processInfo.GetCurrentExecutablePath();
            _registry.SetUserValue(RegistryPathRun, ApplicationAutostartKey, exePath);
            _logger.LogInformation(AutostartCreatedLogMessage);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, ErrorSettingAutostartLogMessage);
            throw new InvalidOperationException(ErrorSettingAutostartLogMessage, exception);
        }
    }

    public Task EnableAutoStartAsync()
    {
        EnableAutoStart();
        return Task.CompletedTask;
    }

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
            _logger.LogError(exception, ErrorCheckingAutostartStatusLogMessage);
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
            _logger.LogWarning(exception, CouldNotReadEdgePolicyLogMessage);
        }

        return true;
    }

    public Dictionary<string, object> RetrieveStartupEntries()
    {
        try
        {
            return _registry.GetUserValues(RegistryPathRun);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, ErrorRetrievingAutostartEntriesLogMessage);
            return [];
        }
    }

    public void SetAutoLogon(bool enable)
    {
        try
        {
            // Logic inverted: Enable AutoLogon means PasswordLess Disabled (0)
            int dwordValue = enable ? DisabledDwordValue : InactiveDwordValue;
            _registry.SetSystemValue(RegistryPathPasswordLess, DevicePasswordLessBuildVersionValueName, dwordValue);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(AutoLogonSettingLogMessage, enable ? ActiveStateText : InactiveStateText, dwordValue);
            }
        }
        catch (UnauthorizedAccessException exception)
        {
            throw new UnauthorizedAccessException(AccessDeniedPasswordLessExceptionMessage, exception);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, ErrorConfiguringAutoLogonLogMessage);
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
                _logger.LogInformation(EdgeStartupBoostLogMessage, enable ? ActiveStateText : InactiveStateText, dwordValue);
            }
        }
        catch (UnauthorizedAccessException exception)
        {
            throw new UnauthorizedAccessException(AccessDeniedEdgePolicyExceptionMessage, exception);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, GeneralErrorEdgePolicyLogMessage);
            throw new InvalidOperationException(FailedEdgePolicyExceptionMessage, exception);
        }
    }
}
