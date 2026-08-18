using System;
using System.Threading;
using Microsoft.Extensions.Logging;

using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Security;
using eBRestarter.Core.Domain.ValueObjects;
using eBRestarter.Core.Application.ObjectArchetypes.Constants;

namespace eBRestarter.Infrastructure.BehavioralComponents.Repositories.Config;

/// <summary>
/// Decorates the base IEVisitorConfigPort to inject transparent encryption and decryption
/// of the sensitive API key during load and save operations (Decorator Pattern).
/// </summary>
/// <param name="inner">The underlying inner configuration repository handling physical I/O.</param>
/// <param name="encryptionUseCase">The outbound encryption port for encrypting and decrypting sensitive configuration values via DPAPI.</param>
/// <param name="logger">The logger instance for telemetry and error tracking.</param>
public sealed class EncryptedEVisitorConfigRepositoryDecorator(
    IOutboundPortEVisitorConfigRepository inner,
    IOutboundPortEncryption encryptionUseCase,
    ILogger<EncryptedEVisitorConfigRepositoryDecorator> logger)
    : IOutboundPortEVisitorConfigRepository
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════

    // ── Block 2: Primitive Types & Strings (alphabetical) ──
    private const string SaveAbortedExceptionMessage = "The configuration was not saved because the API key could not be encrypted.";


    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════

    // ── Block 1: Injected Dependencies (alphabetical) ──
    private readonly IOutboundPortEncryption _encryptionUseCase = encryptionUseCase ?? throw new ArgumentNullException(nameof(encryptionUseCase));
    private readonly IOutboundPortEVisitorConfigRepository _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    private readonly ILogger<EncryptedEVisitorConfigRepositoryDecorator> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    // ⚡ Guide Section 14.1: DPAPI decryption is computationally expensive during LoadConfig.
    // Memoization is performed using the CIPHERTEXT as cache key – the same ciphertext always
    // decrypts to the same plaintext, and an updated API key automatically invalidates the cache
    // because a different ciphertext is received. Memory footprint is bounded to active key versions.
    private readonly Lock _decryptionCacheLock = new();
    private string? _cachedCipherText;
    private string? _cachedPlainText;


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════

    /// <inheritdoc />
    public AppConfig LoadConfig()
    {
        var config = _inner.LoadConfig();

        if (string.IsNullOrEmpty(config.Settings.ApiKey))
        {
            return config;
        }

        try
        {
            var decryptedKey = DecryptCached(config.Settings.ApiKey);
            config.Settings.ApiKey = decryptedKey;

            if (string.IsNullOrEmpty(decryptedKey))
            {
                _logger.LogWarning(LogEventIds.Configuration.ApiKeyDecryptionFailed, "API Key could not be decrypted (possibly changed machine). The key must be re-entered.");
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(LogEventIds.Configuration.ApiKeyDecryptionFailed, exception, "Error while decrypting the API key.");
            config.Settings.ApiKey = string.Empty;
        }

        return config;
    }

    /// <inheritdoc />
    public void ResetConfig()
    {
        _inner.ResetConfig();
    }

    /// <inheritdoc />
    /// <remarks>
    /// 🔒 Security Guidelines Section 8.3 (OWASP A10:2025): If encryption fails, configuration persistence
    /// is aborted and the exception propagated. Silently falling back to plaintext or saving an empty
    /// key is strictly disallowed (Section 2.1).
    /// </remarks>
    public void SaveConfig(AppConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        var originalKey = config.Settings.ApiKey;

        try
        {
            if (!string.IsNullOrEmpty(originalKey))
            {
                var cipherText = _encryptionUseCase.Encrypt(originalKey);
                config.Settings.ApiKey = cipherText;

                // Populate decryption cache with known ciphertext/plaintext pair to avoid redundant DPAPI decryption on subsequent LoadConfig calls.
                StoreDecryptionCache(cipherText, originalKey);
            }
            else
            {
                InvalidateDecryptionCache();
            }

            _inner.SaveConfig(config);
        }
        catch (Exception exception)
        {
            // ⚠️ Exception Guidelines Section 11: Cache invalidation is actual recovery logic. Logging is deferred to the final handling scope.
            InvalidateDecryptionCache();

            throw new InvalidOperationException(SaveAbortedExceptionMessage, exception);
        }
        finally
        {
            config.Settings.ApiKey = originalKey;
        }
    }

    /// <summary>
    /// Decrypts the given cipher text, reusing the previous result when the cipher text is unchanged.
    /// </summary>
    /// <param name="cipherText">The encrypted string value to decrypt.</param>
    /// <returns>The decrypted plaintext string.</returns>
    private string DecryptCached(string cipherText)
    {
        lock (_decryptionCacheLock)
        {
            if (_cachedPlainText is not null && string.Equals(_cachedCipherText, cipherText, StringComparison.Ordinal))
            {
                return _cachedPlainText;
            }
        }

        var plainText = _encryptionUseCase.Decrypt(cipherText);

        if (!string.IsNullOrEmpty(plainText))
        {
            StoreDecryptionCache(cipherText, plainText);
        }

        return plainText;
    }

    /// <summary>
    /// Clears the cached ciphertext and plaintext values.
    /// </summary>
    private void InvalidateDecryptionCache()
    {
        lock (_decryptionCacheLock)
        {
            _cachedCipherText = null;
            _cachedPlainText = null;
        }
    }

    /// <summary>
    /// Stores the ciphertext and corresponding plaintext in the in-memory decryption cache.
    /// </summary>
    /// <param name="cipherText">The ciphertext key.</param>
    /// <param name="plainText">The decrypted plaintext value.</param>
    private void StoreDecryptionCache(string cipherText, string plainText)
    {
        lock (_decryptionCacheLock)
        {
            _cachedCipherText = cipherText;
            _cachedPlainText = plainText;
        }
    }
}
