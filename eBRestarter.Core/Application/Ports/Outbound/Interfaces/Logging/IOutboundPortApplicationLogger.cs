namespace eBRestarter.Core.Application.Ports.Outbound.Interfaces.Logging;

/// <summary>
/// Port: Driven Port (Outbound) for logging structured diagnostic and application event messages without coupling to external frameworks.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für System-Logging)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt im Application Core (z. B. <see cref="Services.ComputerRestartService"/>).<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Infrastructure.Adapters.Logging.MicrosoftExtensionsLoggerAdapter{TCategory}"/> im Infrastructure Layer, der auf <c>Microsoft.Extensions.Logging</c> abbildet).<br/>
/// - <strong>Begründung:</strong> Entkoppelt den Anwendungskern perfekt von externen Logging-Frameworks und I/O-Schreiboperationen (Datei/Konsole) und ist nach Leitfaden ein klassischer <strong>Outbound Port</strong>.<br/>
/// - <em>Architektur-Hinweis (Gelöst):</em> Der Name wurde von <c>IApplicationLoggerPort</c> auf <c>IApplicationLoggerOutboundPort</c> konform zum Leitfaden-Suffix aktualisiert.
/// </para>
/// </summary>
public interface IOutboundPortApplicationLogger<out TCategory>
{
    void LogInformation(string message, params object?[] args);
    void LogWarning(string message, params object?[] args);
    void LogError(Exception? exception, string message, params object?[] args);
}
