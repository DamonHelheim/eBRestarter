using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Core.Application.Ports.Outbound;

/// <summary>
/// Port: Driven Port (Outbound) for retrieving external E-Visitor API data (IP info and earnings) via HTTP.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für HTTP-API)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt in ViewModels und im Application Core zur Abfrage externer Statistik- und IP-Daten.<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Infrastructure.Adapters.EVisitorApiProvider"/> im Infrastructure Layer via HTTP-REST-Client).<br/>
/// - <strong>Begründung:</strong> Da die Schnittstelle im Core definiert ist und die Implementierung im Infrastruktur-Layer auf externe Webservices zugreift, handelt es sich nach Abschnitt 1 des Leitfadens zwingend um einen <strong>Outbound Port</strong>.<br/>
/// - <em>Architektur-Hinweis:</em> Suffix <c>OutboundPort</c> ist vorbildlich. Zur besseren Strukturierung sollte die Datei idealerweise in einen Unterordner wie <c>Ports/Outbound/Network/</c> oder <c>Ports/Outbound/Api/</c> verschoben werden.
/// </para>
/// </summary>
public interface IOutboundPortEVisitorApiProvider
{
    /// <summary>
    /// Retrieves IP information data.
    /// </summary>
    Task<IpInfoData?> RetrieveIpInfoAsync();

    /// <summary>
    /// Retrieves earnings data (username/key are retrieved from the configuration known by the service).
    /// </summary>
    Task<EarningsData?> RetrieveEarningsAsync();
}
