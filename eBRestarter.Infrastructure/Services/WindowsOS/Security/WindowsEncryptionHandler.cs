using eBRestarter.Core.Application.Interfaces.Security;
using System.Security.Cryptography;
using System.Text;

namespace eBRestarter.Infrastructure.Services.WindowsOS.Security;

public class WindowsEncryptionHandler : IEncryptionUseCase
{
    // Optional: Ein zusÃ¤tzlicher "Salz"-Wert (Entropy), damit nur diese App entschlÃ¼sseln kann
    private static readonly byte[] _entropy = Encoding.UTF8.GetBytes("eBRestarter_Secure_Entropy_2026");

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;

        try
        {
            var plainBytes = Encoding.UTF8.GetBytes(plainText);

            // VerschlÃ¼sselung fÃ¼r den aktuellen Benutzer (CurrentUser)
            var encryptedBytes = ProtectedData.Protect(plainBytes, _entropy, DataProtectionScope.CurrentUser);

            return Convert.ToBase64String(encryptedBytes);
        }
        catch
        {
            // Fallback oder Logging im Fehlerfall
            return string.Empty;
        }
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return string.Empty;

        try
        {
            var encryptedBytes = Convert.FromBase64String(cipherText);

            // EntschlÃ¼sselung
            var plainBytes = ProtectedData.Unprotect(encryptedBytes, _entropy, DataProtectionScope.CurrentUser);

            return Encoding.UTF8.GetString(plainBytes);
        }
        catch
        {
            // Tritt auf, wenn Datei auf anderen PC kopiert wurde oder Daten korrupt sind.
            // Wir geben leeren String zurÃ¼ck, damit der User den Key neu eingeben muss.
            return string.Empty;
        }
    }
}

