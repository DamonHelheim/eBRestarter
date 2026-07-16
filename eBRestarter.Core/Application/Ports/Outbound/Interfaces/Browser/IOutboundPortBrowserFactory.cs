using eBRestarter.Core.Application.ObjectArchetypes.Enums;

namespace eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;

/// <summary>
/// Port: Driven Port (Outbound) for creating and retrieving concrete browser automation instances by browser type.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für Browser-Instanziierung)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt INNERHALB des Application Cores (<see cref="UseCases.DeleteBrowserContentUseCase"/> etc.) sowie in UI-ViewModels.<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Infrastructure.Factories.BrowserFactory"/> im Infrastructure Layer via DI/Reflection).<br/>
/// - <strong>Begründung:</strong> Entkoppelt den Anwendungskern von der konkreten Instanziierung und Anbindung OS-spezifischer Webbrowser-Steuerungen und ist somit nach Leitfaden ein vorbildlicher <strong>Outbound Port</strong>.<br/>
/// - <em>Architektur-Hinweis:</em> Suffix <c>OutboundPort</c> ist perfekt.
/// </para>
/// </summary>
public interface IOutboundPortBrowserFactory
{
    IOutboundPortBrowser Create(BrowserType type);
}



