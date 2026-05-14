using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.Security;
using eBRestarter.Core.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace eBRestarter.Infrastructure.Services.Config;

/// <summary>
/// Dekoriert den Basis-IEVisitorConfigService, um transparente Verschlüsselung/Entschlüsselung
/// des sensiblen API-Schlüssels beim Laden und Speichern hinzuzufügen (Decorator-Pattern).
/// </summary>
public class EncryptedEVisitorConfigServiceDecorator(
    IEVisitorConfigService inner,
    IEncryptionService encryptionService,
    ILogger<EncryptedEVisitorConfigServiceDecorator> logger) : IEVisitorConfigService
{
    private readonly IEVisitorConfigService _inner = inner;
    private readonly IEncryptionService _encryptionService = encryptionService;
    private readonly ILogger<EncryptedEVisitorConfigServiceDecorator> _logger = logger;

    /// <summary>
    /// Lädt die Konfiguration und entschlüsselt den API-Key transparent.
    /// </summary>
    public AppConfig LoadConfig()
    {
        var config = _inner.LoadConfig();

        if (!string.IsNullOrEmpty(config.Settings.ApiKey))
        {
            try
            {
                var decryptedKey = _encryptionService.Decrypt(config.Settings.ApiKey);
                config.Settings.ApiKey = decryptedKey;

                if (string.IsNullOrEmpty(decryptedKey))
                {
                    _logger.LogWarning("API Key konnte nicht entschlüsselt werden (evtl. PC gewechselt). Der Key muss neu eingegeben werden.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Entschlüsseln des API-Keys.");
                config.Settings.ApiKey = string.Empty;
            }
        }

        return config;
    }

    /// <summary>
    /// Verschlüsselt den API-Key transparent und delegiert das Speichern an den inneren Service.
    /// Der Klartext-Key bleibt im Speicher erhalten.
    /// </summary>
    public void SaveConfig(AppConfig config)
    {
        var originalKey = config.Settings.ApiKey;
        try
        {
            if (!string.IsNullOrEmpty(originalKey))
            {
                config.Settings.ApiKey = _encryptionService.Encrypt(originalKey);
            }

            _inner.SaveConfig(config);
        }
        finally
        {
            // Stellt den Klartext-Key im Speicher wieder her
            config.Settings.ApiKey = originalKey;
        }
    }

    /// <summary>
    /// Delegiert das Zurücksetzen an den inneren Service.
    /// </summary>
    public void ResetConfig()
    {
        _inner.ResetConfig();
    }
}
