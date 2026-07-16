using eBRestarter.Core.Application.ObjectArchetypes.DTOs.ImmutableSnapshot;
using eBRestarter.Core.Domain.ValueObjects;

namespace eBRestarter.Core.Application.Ports.Inbound.Interfaces.Handlers;

/// <summary>
/// Port: Generates the initial display state for the restart task UI from configuration and localization.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): INBOUND PORT / USE CASE INTERFACE</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Desktop.WinUI3.ViewModels.ViewModelRestartTask"/> im Presentation Layer via MVVM).<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt INNERHALB des Application Cores (<see cref="BehavioralComponents.Handlers.RestartTaskDisplayStateHandler"/>).<br/>
/// - <strong>Begründung:</strong> Dient der Desktop-Benutzeroberfläche als Inbound Port, um den initialen Anzeigezustand der Neustartaufgabe für die View zu aufzubereiten.
/// </para>
/// </summary>
public interface IInboundPortRestartTaskDisplayStateHandler
{
    /// <summary>
    /// Builds the display DTO from the current configuration (including cache deletion status and formatted texts).
    /// </summary>
    RestartTaskDisplayState RetrieveInitialState(AppConfig config);
}