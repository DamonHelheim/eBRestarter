using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Security;
using System.Security.Cryptography;
using System.Text;

namespace eBRestarter.Infrastructure.Adapters.Outbound.WindowsOS.Security;

/// <summary>
/// Adapter: Driven Adapter (Outbound Handler/Adapter) for encrypting and decrypting strings using Windows DPAPI.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Baustein im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core zur Datenverschlüsselung via <see cref="ProtectedData"/>.<br/>
/// - <strong>Implementierter Port:</strong> <see cref="IOutboundPortEncryption"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein vorbildlicher <strong>Outbound Adapter</strong>, da sie im Infrastructure-Layer liegt, einen Outbound Port implementiert und vom Core angetrieben wird, um native OS-Kryptographie auszuführen.
/// </para>
/// </summary>
public sealed class AdapterWindowsEncryption : IOutboundPortEncryption
{
    // Optional: An additional "salt" value (entropy) ensuring only this application can handle decryption tasks
    private static readonly byte[] _entropy = Encoding.UTF8.GetBytes("eBRestarter_Secure_Entropy_2026");

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;

        try
        {
            var plainBytes = Encoding.UTF8.GetBytes(plainText);

            // Data encryption constrained specifically to the context of the CurrentUser
            var encryptedBytes = ProtectedData.Protect(plainBytes, _entropy, DataProtectionScope.CurrentUser);

            return Convert.ToBase64String(encryptedBytes);
        }
        catch
        {
            // Fallback or error logging hook in case of failure
            return string.Empty;
        }
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return string.Empty;

        try
        {
            var encryptedBytes = Convert.FromBase64String(cipherText);

            // Data decryption execution
            var plainBytes = ProtectedData.Unprotect(encryptedBytes, _entropy, DataProtectionScope.CurrentUser);

            return Encoding.UTF8.GetString(plainBytes);
        }
        catch
        {
            // Triggered if the file was migrated to another workstation machine environment or if data structures are malformed/corrupted.
            // Returning an empty string forces the user interface to request clean key re-entry gracefully.
            return string.Empty;
        }
    }
}