using eBRestarter.Core.Application.Ports.Outbound.Config;
using eBRestarter.Core.Application.Ports.Outbound.Security;
using eBRestarter.Core.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace eBRestarter.Infrastructure.Repositories.Config;

/// <summary>
/// Decorates the base IEVisitorConfigPort to inject transparent encryption and decryption
/// of the sensitive API key during load and save operations (Decorator Pattern).
/// </summary>
public sealed class EncryptedEVisitorConfigRepositoryDecorator(
    IOutboundPortEVisitorConfigRepository inner,
    IOutboundPortEncryption encryptionUseCase,
    ILogger<EncryptedEVisitorConfigRepositoryDecorator> logger) : IOutboundPortEVisitorConfigRepository
{
    private readonly IOutboundPortEVisitorConfigRepository _inner = inner;
    private readonly IOutboundPortEncryption _encryptionUseCase = encryptionUseCase;
    private readonly ILogger<EncryptedEVisitorConfigRepositoryDecorator> _logger = logger;

    /// <summary>
    /// Loads the configuration and transparently decrypts the API key.
    /// </summary>
    public AppConfig LoadConfig()
    {
        var config = _inner.LoadConfig();

        if (!string.IsNullOrEmpty(config.Settings.ApiKey))
        {
            try
            {
                var decryptedKey = _encryptionUseCase.Decrypt(config.Settings.ApiKey);
                config.Settings.ApiKey = decryptedKey;

                if (string.IsNullOrEmpty(decryptedKey))
                {
                    _logger.LogWarning("API Key could not be decrypted (possibly changed machine). The key must be re-entered.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while decrypting the API key.");
                config.Settings.ApiKey = string.Empty;
            }
        }

        return config;
    }

    /// <summary>
    /// Transparently encrypts the API key and delegates the saving to the inner service.
    /// The plain text key remains preserved in memory.
    /// </summary>
    public void SaveConfig(AppConfig config)
    {
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
            // Restores the plain text key in memory
            config.Settings.ApiKey = originalKey;
        }
    }

    /// <summary>
    /// Delegates the configuration reset to the inner service.
    /// </summary>
    public void ResetConfig()
    {
        _inner.ResetConfig();
    }
}
