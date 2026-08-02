using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

namespace eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;

/// <summary>
/// Port: Use Case Interface for inspecting and toggling Microsoft Edge Startup Boost settings from the presentation layer.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): INBOUND PORT (Use Case Interface)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt AUßERHALB des Application Cores im Presentation Layer via MVVM.<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt INNERHALB des Application Cores (<see cref="Application.UseCases.ToggleEdgeStartupBoostUseCase"/>).<br/>
/// - <strong>Begründung:</strong> Dient der Benutzeroberfläche als Eingangstür (Inbound Port) in den Anwendungskern, um den Status des Edge Startup Boosts abzufragen und zu umschalten.<br/>
/// </para>
/// </summary>
public interface IUseCaseToggleEdgeStartupBoost
{
    bool IsEnabled();
    bool IsEdgeInstalled();
    ToggleEdgeStartupBoostResponse Toggle(bool shouldEnable);
}
