using System.Net.NetworkInformation;

namespace eBRestarter.Core.Application.Ports.Outbound.Interfaces.Network;

/// <summary>
/// Port: Driven Port (Outbound) for querying system network availability and hardware network interface adapters.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für OS-Netzwerkstatus)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt in der Anwendungs- oder Infrastrukturlogik zur Ermittlung des Netzwerkstatus (<see cref="eBRestarter.Infrastructure.Providers.NetworkInfoProvider"/>).<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Infrastructure.Adapters.WindowsOS.WindowsNetworkProvider"/> via OS-Netzwerk-APIs).<br/>
/// - <strong>Begründung:</strong> Abstrahiert die statischen Systemaufrufe des Betriebssystems bezüglich Netzwerkverbindung und -adaptern und ist somit nach Leitfaden ein vorbildlicher <strong>Outbound Port</strong>.<br/>
/// - <em>Architektur-Hinweis:</em> Suffix <c>OutboundPort</c> ist perfekt. Hinweis: Die Rückgabe des BCL-Typs <see cref="NetworkInterface"/> verbindet die Schnittstelle leicht mit .NET-spezifischen Netzwerk-Infrastrukturklassen, was jedoch in einer reinen .NET-Umgebung akzeptabel ist.
/// </para>
/// </summary>
public interface IOutboundPortNetworkProvider
{
    bool CheckIsNetworkAvailable();
    NetworkInterface[] RetrieveAllNetworkInterfaces();
}


