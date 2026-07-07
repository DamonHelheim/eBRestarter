using System;

namespace eBRestarter.Core.Application.Ports.Outbound.Application;

/// <summary>
/// Port: Driven Port (Outbound) for retrieving the application version string from the operating system host.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für App-Version)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt in der Benutzeroberfläche (<see cref="eBRestarter.Desktop.WinUI3.Views.Windows.eBRestarter"/> im Hauptfenster-Titel via DI).<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Infrastructure.Adapters.WindowsOS.WindowsAppVersionInfoProvider"/> via <c>Assembly.GetExecutingAssembly</c>).<br/>
/// - <strong>Begründung:</strong> Da die Implementierung extern in der Infrastruktur auf OS-/Assembly-Daten zugreift, handelt es sich nach Abschnitt 1 des Leitfadens um einen vorbildlichen <strong>Outbound Port</strong>.<br/>
/// - <em>Architektur-Hinweis (Gelöst):</em> Diese Schnittstelle wurde aus Redundanzgründen mit <see cref="IAppInfoProviderOutboundPort"/> zusammengeführt und erbt nun von ihr, um die doppelte Signaturdefinition aufzulösen.
/// </para>
/// </summary>
[Obsolete("Use IAppInfoProviderOutboundPort instead. This port has been unified to eliminate redundancy.")]
public interface IAppVersionInfoProviderOutboundPort : IAppInfoProviderOutboundPort
{
}

