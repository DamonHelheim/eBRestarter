namespace eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;

/// <summary>
/// Port: Provides localized string resources from the environment/UI platform to the application.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt INNERHALB des Application Cores (<see cref="Application.Services.RestarterCycleService"/>, Use Cases) sowie im Presentation Layer.<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Desktop.WinUI3.Providers.LocalizationProvider"/> in der Infrastruktur/UI-Host).<br/>
/// - <strong>Begründung:</strong> Nach Leitfaden (Abschnitt 1) ist eine im Core definierte Schnittstelle, deren Implementierung außerhalb des Cores liegt (Zugriff auf OS/Resx-Ressourcen), zwingend ein <strong>Outbound Port</strong>.<br/>
/// - <em>Architektur-Hinweis:</em> Befindet sich derzeit unter <c>Inbound/Providers</c>, gehört architektonisch jedoch in den Bereich <c>Outbound</c> (z. B. als <c>ILocalizationProviderOutboundPort</c>).
/// </para>
/// </summary>
public interface IInboundPortLocalizationProvider
{
    string RetrieveString(string key);
}
