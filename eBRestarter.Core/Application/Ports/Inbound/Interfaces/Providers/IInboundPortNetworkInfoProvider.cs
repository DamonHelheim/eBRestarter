using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

namespace eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;

/// <summary>
/// Port: Exposes active network statistics and availability status to the presentation layer.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): INBOUND PORT / USE CASE INTERFACE</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Desktop.WinUI3.ViewModels.ViewModelNetworkTraffic"/> im Presentation Layer via MVVM).<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt derzeit in der Infrastruktur (<see cref="eBRestarter.Infrastructure.Providers.NetworkInfoProvider"/>).<br/>
/// - <strong>Begründung:</strong> Dient der Benutzeroberfläche als Inbound Port zum Abruf und zur Anzeige der Netzwerk-Verkehrsdaten.<br/>
/// - <em>Architektur-Hinweis:</em> Dass die Implementierung direkt in <c>Infrastructure</c> statt im <c>Application Core</c> liegt, stellt laut Leitfaden Abschnitt 1 einen Verstoß gegen die saubere Schichtentrennung dar (Inbound Ports sollten im Core implementiert werden).
/// </para>
/// </summary>
public interface IInboundPortNetworkInfoProvider
{
    bool IsNetworkAvailable();
    IEnumerable<NetworkStats> RetrieveActiveInterfaces();
}
