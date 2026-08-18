using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

namespace eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;

/// <summary>
/// Normalizes persisted config on process launch (e.g. browser cache delete date) and exposes UI preferences.
/// </summary>
/// <remarks>
/// <b>Architectural Classification: INBOUND PORT / USE CASE INTERFACE</b>
/// <list type="bullet">
/// <item><b>Consumer:</b> Presentation layer (<see cref="eBRestarter.Desktop.WinUI3.App"/> at application launch).</item>
/// <item><b>Implementer:</b> Application Core (<see cref="BehavioralComponents.Providers.StartupConfigProvider"/>).</item>
/// <item><b>Rationale:</b> Serves as an entry point for UI bootstrapping to provide initial display preferences (language, theme).</item>
/// </list>
/// </remarks>
public interface IInboundPortStartupConfigProvider
{
    /// <summary>
    /// Returns language and theme for WinUI bootstrap by reading the configuration.
    /// </summary>
    /// <returns>A <see cref="StartupDisplayPreferences"/> instance containing language and theme settings.</returns>
    StartupDisplayPreferences RetrieveStartupPreferences();
}

