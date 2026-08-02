using System;
using System.Security.Cryptography;
using System.Text;

using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Security;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Utility;

/// <summary>
/// Adapter: Driven Adapter (Outbound Handler/Adapter) for encrypting and decrypting strings using Windows DPAPI.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Baustein im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core zur Datenverschlüsselung via <see cref="ProtectedData"/>.<br/>
/// - <strong>Implementierter Port:</strong> <see cref="IOutboundPortEncryption"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein vorbildlicher <strong>Outbound Adapter</strong>, da sie im Infrastructure-Layer liegt, einen Outbound Port implementiert und vom Core angetrieben wird, um native OS-Kryptographie auszuführen.
/// </para>
/// </summary>
public sealed class AdapterWindowsEncryptionUtility : IOutboundPortEncryption
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitive Typen & Strings ──
    private static readonly byte[] Entropy = "eBRestarter_Secure_Entropy_2026"u8.ToArray();


    // ═══════════════════════════════════════════════════════
    //  8. Methods (public → private)
    // ═══════════════════════════════════════════════════════
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
        catch (CryptographicException)
        {
            // Triggered if the file was migrated to another workstation machine environment or if data structures are malformed/corrupted.
            // Returning an empty string forces the user interface to request clean key re-entry gracefully.
            return string.Empty;
        }
        catch (FormatException)
        {
            // Triggered if cipherText is not valid base64
            return string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

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
        catch (CryptographicException)
        {
            // Expected fallback when encryption fails or key is invalid
            return string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
}