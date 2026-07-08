using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

namespace eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;

/// <summary>
/// Port: Retrieves aggregated system hardware, OS, and runtime information for display in the client UI.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): INBOUND PORT / USE CASE INTERFACE</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Desktop.WinUI3.ViewModels.ViewModelInfocenter"/> im Presentation Layer via MVVM).<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt INNERHALB des Application Cores (<see cref="Application.Providers.SystemInformationProvider"/>).<br/>
/// - <strong>Begründung:</strong> Dient als Eingangstür in den Anwendungskern für das Infocenter, um der Benutzeroberfläche die System- und Laufzeitinformationen aufzubereiten und zur Verfügung zu stellen.
/// </para>
/// </summary>
public interface IInboundPortSystemInformationProvider
{
    Task<SystemInformationResponse> RetrieveAsync();
}
