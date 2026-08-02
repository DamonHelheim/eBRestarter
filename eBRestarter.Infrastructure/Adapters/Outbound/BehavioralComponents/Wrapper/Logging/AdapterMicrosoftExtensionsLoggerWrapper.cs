using System;
using Microsoft.Extensions.Logging;

using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Logging;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Wrapper.Logging;

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
public sealed class AdapterMicrosoftExtensionsLoggerWrapper<TCategory>(
    ILogger<TCategory> logger)
    : IOutboundPortApplicationLogger<TCategory>
#pragma warning restore S6672 // Generic logger injection should match enclosing type
{
    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════

    // ── Block 1: Injizierte Abhängigkeiten (alphabetisch) ──
    private readonly ILogger<TCategory> _logger = logger ?? throw new ArgumentNullException(nameof(logger));


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════

#pragma warning disable CA2254 // Template should be a static expression
    public void LogError(Exception? exception, string message, params object?[] args) => _logger.LogError(exception, message, args);

    public void LogInformation(string message, params object?[] args) => _logger.LogInformation(message, args);

    public void LogWarning(string message, params object?[] args) => _logger.LogWarning(message, args);
#pragma warning restore CA2254 // Template should be a static expression
}
