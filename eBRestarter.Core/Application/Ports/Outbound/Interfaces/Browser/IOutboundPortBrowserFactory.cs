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
    /// <summary>
    /// Creates or retrieves the browser automation wrapper for the specified browser type.
    /// </summary>
    /// <param name="type">The targeted browser type.</param>
    /// <returns>An <see cref="IOutboundPortBrowser"/> implementation for the requested browser.</returns>
    /// <exception cref="NotSupportedException">Thrown when the requested <paramref name="type"/> is not supported.</exception>
    IOutboundPortBrowser Create(BrowserType type);
}
