using eBRestarter.Core.Application.Ports.Outbound.Logging;
using Microsoft.Extensions.Logging;

namespace eBRestarter.Infrastructure.Adapters.Logging;

/// <summary>
/// Adapter: Driven Adapter (Outbound Handler/Adapter) wrapping Microsoft.Extensions.Logging for domain-agnostic logging.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Baustein im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core zur Systemprotokollierung (Logging).<br/>
/// - <strong>Implementierter Port:</strong> <see cref="IApplicationLoggerOutboundPort{TCategory}"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein vorbildlicher <strong>Outbound Adapter</strong>, da sie im Infrastructure-Layer liegt und einen Outbound Port implementiert, um externes Logging zu kapseln.
/// </para>
/// </summary>
public sealed class MicrosoftExtensionsLoggerAdapter<TCategory>(ILogger<TCategory> logger) : IApplicationLoggerOutboundPort<TCategory>
{
    public void LogInformation(string message, params object?[] args) => logger.LogInformation(message, args);

    public void LogWarning(string message, params object?[] args) => logger.LogWarning(message, args);

    public void LogError(Exception? exception, string message, params object?[] args) => logger.LogError(exception, message, args);
}
