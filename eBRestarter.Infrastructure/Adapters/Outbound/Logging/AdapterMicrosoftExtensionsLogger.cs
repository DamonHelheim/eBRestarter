using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Logging;
using Microsoft.Extensions.Logging;

namespace eBRestarter.Infrastructure.Adapters.Outbound.Logging;

/// <summary>
/// Adapter: Driven Adapter (Outbound Handler/Adapter) wrapping Microsoft.Extensions.Logging for domain-agnostic logging.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Baustein im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core zur Systemprotokollierung (Logging).<br/>
/// - <strong>Implementierter Port:</strong> <see cref="IOutboundPortApplicationLogger{TCategory}"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein vorbildlicher <strong>Outbound Adapter</strong>, da sie im Infrastructure-Layer liegt und einen Outbound Port implementiert, um externes Logging zu kapseln.
/// </para>
/// </summary>
#pragma warning disable S6672 // Generic logger injection should match enclosing type
public sealed class AdapterMicrosoftExtensionsLogger<TCategory>(ILogger<TCategory> logger) : IOutboundPortApplicationLogger<TCategory>
#pragma warning restore S6672 // Generic logger injection should match enclosing type
{
    public ILogger<TCategory> Logger { get; } = logger;

    public void LogInformation(string message, params object?[] args) => Logger.LogInformation(message, args);

    public void LogWarning(string message, params object?[] args) => Logger.LogWarning(message, args);

    public void LogError(Exception? exception, string message, params object?[] args) => Logger.LogError(exception, message, args);
}
