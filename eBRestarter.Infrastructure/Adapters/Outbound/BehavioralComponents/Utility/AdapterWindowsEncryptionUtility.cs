using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;
using System.Text;

using eBRestarter.Core.Application.ObjectArchetypes.Constants;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Security;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Utility;

/// <summary>
/// Adapter: Driven Adapter (Outbound Handler/Adapter) for encrypting and decrypting strings using Windows DPAPI.
/// <para>
/// <strong>Architecture Classification: OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Role &amp; Responsibility:</strong> Encrypts and decrypts sensitive data via Windows DPAPI (<see cref="ProtectedData"/>) in the Infrastructure layer.<br/>
/// - <strong>Implemented Port:</strong> <see cref="IOutboundPortEncryption"/>.<br/>
/// </para>
/// </summary>
/// <param name="logger">Logger instance.</param>
public sealed class AdapterWindowsEncryptionUtility(
    ILogger<AdapterWindowsEncryptionUtility> logger) : IOutboundPortEncryption
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitives & strings ──
    private const string EncryptionFailedExceptionMessage = "The value could not be encrypted with Windows DPAPI. It was deliberately NOT persisted unprotected.";

    /// <summary>
    /// Additional entropy mixed into every DPAPI operation.
    /// </summary>
    /// <remarks>
    /// Security &amp; Design Rationale: Additional entropy acts as an application-specific salt to isolate encrypted values within the CurrentUser DPAPI scope.
    /// </remarks>
    private static readonly byte[] Entropy = "eBRestarter_Secure_Entropy_2026"u8.ToArray();


    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injected dependencies ──
    private readonly ILogger<AdapterWindowsEncryptionUtility> _logger = logger ?? throw new ArgumentNullException(nameof(logger));


    // ═══════════════════════════════════════════════════════
    //  8. Methods (public → private)
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Decrypts a Base64-encoded DPAPI cipher text string.
    /// </summary>
    /// <param name="cipherText">Base64-encoded DPAPI cipher text string.</param>
    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrWhiteSpace(cipherText))
        {
            return string.Empty;
        }

        try
        {
            var encryptedBytes = Convert.FromBase64String(cipherText);

            // Data decryption execution
            var plainBytes = ProtectedData.Unprotect(encryptedBytes, Entropy, DataProtectionScope.CurrentUser);

            return Encoding.UTF8.GetString(plainBytes);
        }
        catch (CryptographicException exception)
        {
            // Triggered if the file was migrated to another workstation machine environment or if data structures are malformed/corrupted.
            // Returning an empty string forces the user interface to request clean key re-entry gracefully.
            // Log diagnostic warning for decryption failure before returning fallback empty string.
            _logger.LogWarning(
                LogEventIds.Configuration.ApiKeyDecryptionFailed,
                exception,
                "DPAPI could not unprotect the stored value; the profile was most likely copied from another machine or user.");

            return string.Empty;
        }
        catch (FormatException exception)
        {
            _logger.LogWarning(
                LogEventIds.Configuration.ApiKeyDecryptionFailed,
                exception,
                "The stored cipher text is not valid Base64 and could not be decrypted.");

            return string.Empty;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                LogEventIds.Configuration.ApiKeyDecryptionFailed,
                exception,
                "Unexpected failure while decrypting a protected value.");

            return string.Empty;
        }
    }

    /// <summary>
    /// Protects the given plain text with Windows DPAPI in the current user's scope.
    /// </summary>
    /// <param name="plainText">Plain text string to encrypt.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when DPAPI cannot protect the value. Callers must treat this as a hard failure.
    /// </exception>
    /// <remarks>
    /// Security &amp; Exception Handling: Rethrows InvalidOperationException on DPAPI encryption failure to prevent silent data loss (OWASP A10:2025).
    /// </remarks>
    public string Encrypt(string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
        {
            return string.Empty;
        }

        try
        {
            var plainBytes = Encoding.UTF8.GetBytes(plainText);

            // Data encryption constrained specifically to the context of the CurrentUser
            var encryptedBytes = ProtectedData.Protect(plainBytes, Entropy, DataProtectionScope.CurrentUser);

            return Convert.ToBase64String(encryptedBytes);
        }
        catch (Exception exception)
        {
            // Exception handling: Wrap and rethrow exception without logging locally to prevent duplicate log entries.
            throw new InvalidOperationException(EncryptionFailedExceptionMessage, exception);
        }
    }
}