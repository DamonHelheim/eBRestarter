namespace eBRestarter.Core.Application.Ports.Outbound.Application;

/// <summary>
/// Port: Driven Port (Outbound) for retrieving assembly version and runtime application metadata from the host environment.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für App-Metadaten)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt in der Benutzeroberfläche (<see cref="eBRestarter.Desktop.WinUI3.ViewModels.ViewModelAbout"/>).<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Infrastructure.Adapters.AppInfoProvider"/> im Infrastructure Layer via Assembly-Reflection/OS-API).<br/>
/// - <strong>Begründung:</strong> Da die Implementierung extern in der Infrastruktur auf physische Assembly-Attribute zugreift, handelt es sich nach Abschnitt 1 des Leitfadens um einen vorbildlichen <strong>Outbound Port</strong>.<br/>
/// - <em>Architektur-Hinweis:</em> Suffix <c>OutboundPort</c> ist vorbildlich.
/// </para>
/// </summary>
public interface IAppInfoProviderOutboundPort
{
    string RetrieveAppVersion();
}


