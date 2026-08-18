using System;

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
/// <remarks>
/// <para>
/// 📝 <b>Verhältnis zur Logging-Guideline Kap. 2 — bewusste, dokumentierte Abweichung:</b>
/// Kap. 2 verlangt, dass Anwendungscode direkt von <c>ILogger&lt;T&gt;</c>
/// (<c>Microsoft.Extensions.Logging.Abstractions</c>) abhängt. Für diesen Core gilt jedoch die
/// übergeordnete Projektvorgabe "keine externen Frameworks im Core" (Hexagonal Architecture).
/// Dieser Port ist die Auflösung des Konflikts: Der Core kennt nur die eigene Abstraktion, der
/// Adapter im Infrastructure-Layer bildet sie auf <c>ILogger&lt;T&gt;</c> ab. Das Ziel von Kap. 2 —
/// Provider-Unabhängigkeit der Business-Logik — ist damit erfüllt, sogar strenger als gefordert.
/// </para>
/// <para>
/// <b>Preis dieser Entscheidung:</b> Source-Generated Logging nach Kap. 3
/// (<c>[LoggerMessage]</c>) setzt zwingend <c>Microsoft.Extensions.Logging.ILogger</c> voraus und
/// ist im Core deshalb technisch nicht verfügbar. In Infrastructure und Desktop wird es eingesetzt.
/// </para>
/// <para>
/// 📝 <b>Kap. 6 (EventId-Konventionen):</b> Jede Methode nimmt eine <c>eventId</c> entgegen —
/// bewusst als <c>int</c> und nicht als <c>Microsoft.Extensions.Logging.EventId</c>, damit die
/// Framework-Freiheit des Cores erhalten bleibt. Die gültigen Werte stehen zentral in
/// <see cref="ObjectArchetypes.Constants.LogEventIds"/>.
/// </para>
/// <para>
/// 📝 <b>Kap. 4 (Strukturiertes Logging):</b> <c>message</c> ist immer ein literales Template mit
/// benannten Platzhaltern (<c>"… {Target} …"</c>) — niemals ein interpolierter String.
/// </para>
/// </remarks>
#pragma warning disable S2326 // Unused type parameters should be removed
public interface IOutboundPortApplicationLogger<out TCategory>
#pragma warning restore S2326 // Unused type parameters should be removed
{
    /// <summary>
    /// Attaches an ambient property to every log entry written inside the returned scope.
    /// </summary>
    /// <param name="key">Property name, PascalCase (Kap. 4), e.g. <c>"RestarterCycleId"</c>.</param>
    /// <param name="value">Property value.</param>
    /// <returns>Dispose to close the scope; may be <see langword="null"/> if the provider has no scope support.</returns>
    /// <remarks>
    /// 📝 Logging-Guideline Kap. 7 (Scopes): Haengt Kontext an alle Log-Aufrufe innerhalb des
    /// Scopes, ohne ihn in jedem einzelnen Aufruf zu wiederholen. Bewusst mit Schluessel/Wert statt
    /// mit einem Dictionary in der Signatur - so bleibt der Port ohne Framework-Typen.
    /// </remarks>
    IDisposable? BeginScope(string key, object? value);

    /// <summary>Diagnostic detail, disabled in production by default (Kap. 5).</summary>
    void LogDebug(int eventId, string message, params object?[] args);

    /// <summary>Significant business milestone (Kap. 5) — not every intermediate step.</summary>
    void LogInformation(int eventId, string message, params object?[] args);

    /// <summary>Unexpected, but operation continues in a degraded state (Kap. 5).</summary>
    void LogWarning(int eventId, string message, params object?[] args);

    /// <summary>As <see cref="LogWarning(int, string, object?[])"/>, with the causing exception attached.</summary>
    void LogWarning(int eventId, Exception? exception, string message, params object?[] args);

    /// <summary>Operation failed, the application keeps running (Kap. 5).</summary>
    void LogError(int eventId, Exception? exception, string message, params object?[] args);

    /// <summary>Application-wide failure requiring immediate attention (Kap. 5).</summary>
    void LogCritical(int eventId, Exception? exception, string message, params object?[] args);
}
