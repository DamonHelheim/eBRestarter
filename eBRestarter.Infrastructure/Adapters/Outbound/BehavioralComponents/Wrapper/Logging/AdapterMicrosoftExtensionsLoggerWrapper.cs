using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Logging;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Wrapper.Logging;

/// <summary>
/// Adapter: Driven Adapter (Outbound Handler/Adapter) wrapping Microsoft.Extensions.Logging for domain-agnostic logging.
/// <para>
/// <strong>Architecture Classification: OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Role &amp; Responsibility:</strong> Wraps Microsoft.Extensions.Logging for system logging in the Infrastructure layer.<br/>
/// - <strong>Implemented Port:</strong> <see cref="IOutboundPortApplicationLogger{TCategory}"/>.<br/>
/// </para>
/// </summary>
/// <remarks>
/// Translates domain event IDs to Microsoft.Extensions.Logging EventId structures.
/// CA2254 is suppressed as log message templates originate from static constants passed via interface calls.
/// </remarks>
/// <param name="logger">Framework logger instance.</param>
#pragma warning disable S6672 // Generic logger injection should match enclosing type
public sealed class AdapterMicrosoftExtensionsLoggerWrapper<TCategory>(
    ILogger<TCategory> logger)
    : IOutboundPortApplicationLogger<TCategory>
#pragma warning restore S6672 // Generic logger injection should match enclosing type
{
    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════

    // ── Block 1: Injected dependencies ──
    private readonly ILogger<TCategory> _logger = logger ?? throw new ArgumentNullException(nameof(logger));


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════

    /// <inheritdoc />
    /// <remarks>
    /// Wraps key-value pairs in a dictionary to ensure named property formatting in Serilog and console sinks.
    /// </remarks>
    public IDisposable? BeginScope(string key, object? value)
        => _logger.BeginScope(new Dictionary<string, object?> { [key] = value });

#pragma warning disable CA2254 // Template should be a static expression
    /// <inheritdoc />
    public void LogCritical(int eventId, Exception? exception, string message, params object?[] args)
        => _logger.LogCritical(new EventId(eventId), exception, message, args);

    /// <inheritdoc />
    public void LogDebug(int eventId, string message, params object?[] args)
        => _logger.LogDebug(new EventId(eventId), message, args);

    /// <inheritdoc />
    public void LogError(int eventId, Exception? exception, string message, params object?[] args)
        => _logger.LogError(new EventId(eventId), exception, message, args);

    /// <inheritdoc />
    public void LogInformation(int eventId, string message, params object?[] args)
        => _logger.LogInformation(new EventId(eventId), message, args);

    /// <inheritdoc />
    public void LogWarning(int eventId, string message, params object?[] args)
        => _logger.LogWarning(new EventId(eventId), message, args);

    /// <inheritdoc />
    public void LogWarning(int eventId, Exception? exception, string message, params object?[] args)
        => _logger.LogWarning(new EventId(eventId), exception, message, args);
#pragma warning restore CA2254 // Template should be a static expression
}
