using System;
using Microsoft.Extensions.Logging;

using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Security;
using eBRestarter.Core.Domain.ValueObjects;

namespace eBRestarter.Infrastructure.BehavioralComponents.Repositories.Config;

/// <summary>
/// Decorates the base IEVisitorConfigPort to inject transparent encryption and decryption
/// of the sensitive API key during load and save operations (Decorator Pattern).
/// </summary>
public sealed class EncryptedEVisitorConfigRepositoryDecorator(
    IOutboundPortEVisitorConfigRepository inner,
    IOutboundPortEncryption encryptionUseCase,
    ILogger<EncryptedEVisitorConfigRepositoryDecorator> logger)
    : IOutboundPortEVisitorConfigRepository
{
    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════

    // ── Block 1: Injizierte Abhängigkeiten (alphabetisch) ──
    private readonly IOutboundPortEncryption _encryptionUseCase = encryptionUseCase ?? throw new ArgumentNullException(nameof(encryptionUseCase));
    private readonly IOutboundPortEVisitorConfigRepository _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    private readonly ILogger<EncryptedEVisitorConfigRepositoryDecorator> _logger = logger ?? throw new ArgumentNullException(nameof(logger));


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════

    public AppConfig LoadConfig()
    {
        var config = _inner.LoadConfig();

        if (string.IsNullOrEmpty(config.Settings.ApiKey))
        {
            return config;
        }

        try
        {
            var decryptedKey = _encryptionUseCase.Decrypt(config.Settings.ApiKey);
            config.Settings.ApiKey = decryptedKey;

            if (string.IsNullOrEmpty(decryptedKey))
            {
                _logger.LogWarning("API Key could not be decrypted (possibly changed machine). The key must be re-entered.");
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error while decrypting the API key.");
            config.Settings.ApiKey = string.Empty;
        }

        return config;
    }

    public void ResetConfig()
    {
        _inner.ResetConfig();
    }

    public void SaveConfig(AppConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        var originalKey = config.Settings.ApiKey;

        try
        {
            if (!string.IsNullOrEmpty(originalKey))
            {
                config.Settings.ApiKey = _encryptionUseCase.Encrypt(originalKey);
            }

            _inner.SaveConfig(config);
        }
        finally
        {
            config.Settings.ApiKey = originalKey;
        }
    }
}
