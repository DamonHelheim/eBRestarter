using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

namespace eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;

/// <summary>
/// Normalizes persisted config on process launch (e.g. browser cache delete date) and exposes UI preferences.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): INBOUND PORT / USE CASE INTERFACE</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Desktop.WinUI3.App"/> im Presentation Layer beim Anwendungsstart).<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt INNERHALB des Application Cores (<see cref="BehavioralComponents.Providers.StartupConfigProvider"/>).<br/>
/// - <strong>Begründung:</strong> Dient als Eingangstür in den Anwendungskern für das UI-Bootstrap, um dem Desktop-Client beim Start die initialen Anzeigeeinstellungen (Theme, Sprache) bereitzustellen.
/// </para>
/// </summary>
public interface IInboundPortStartupConfigProvider
{
    /// <summary>
    /// Returns language and theme for WinUI bootstrap by reading the configuration.
    /// </summary>
    StartupDisplayPreferences RetrieveStartupPreferences();
}

