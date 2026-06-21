using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace eBRestarter.Infrastructure.Services.WindowsOS.Security;

[SupportedOSPlatform("windows")]
public partial class WindowsAutoLogonAdapter(ILogger<WindowsAutoLogonAdapter> logger) : IWindowsAutoLogonService
{
    private const string WinLogonPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon";
    private const string DefaultPasswordKey = "DefaultPassword";

    private readonly ILogger<WindowsAutoLogonAdapter> _logger = logger;

    public void EnableAutoLogon(string username, string domain, string password)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(WinLogonPath, true) ?? throw new InvalidOperationException("Winlogon Registry Key nicht gefunden.");

            // 1. Registry Werte setzen
            key.SetValue("AutoAdminLogon", "1", RegistryValueKind.String);

            key.SetValue("DefaultUserName", username, RegistryValueKind.String);

            if (string.IsNullOrEmpty(domain))
            {
                key.DeleteValue("DefaultDomainName", false);
            }
            else
            {
                key.SetValue("DefaultDomainName", domain, RegistryValueKind.String);
            }

            // WICHTIG: Klartext-Passwort aus Registry lÃ¶schen, falls vorhanden
            key.DeleteValue(DefaultPasswordKey, false);

            // 2. Passwort sicher in LSA Secrets speichern
            SetLsaSecret(DefaultPasswordKey, password);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("AutoLogon fÃ¼r User '{User}' erfolgreich aktiviert.", username);
            }

        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Fehler beim Aktivieren von AutoLogon.", ex);
        }
    }

    public void DisableAutoLogon()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(WinLogonPath, true);

            key?.SetValue("AutoAdminLogon", "0", RegistryValueKind.String);

            // LSA Secret lÃ¶schen
            SetLsaSecret(DefaultPasswordKey, null);

            _logger.LogInformation("AutoLogon deaktiviert.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Deaktivieren von AutoLogon.");
        }
    }

    public bool IsWindowsHelloPasswordlessEnabled()
    {
        try
        {
            // Der genaue Pfad, in dem Windows 11 die "Nur Windows Hello zulassen"-Einstellung speichert
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\PasswordLess\Device", false);

            if (key is not null)
            {
                var val = key.GetValue("DevicePasswordLessBuildVersion");

                // Wenn der Wert '2' ist, hat der Nutzer die Option AKTIVIERT (Passwort-Login ist blockiert).
                // Wenn der Wert '0' ist (oder nicht existiert), ist sie DEAKTIVIERT.
                if (val is int intValue && intValue == 2)
                {
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Lesen des PasswordLess Registry-Keys.");
        }

        return false;
    }

    public void SetWindowsHelloPasswordlessState(bool enable)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\PasswordLess\Device", true);

            if (key is null)
            {
                _logger.LogWarning("Registry-Key fÃ¼r PasswordLess Device nicht gefunden. Erstelle ihn nicht neu, da dies systemspezifisch ist.");
            }
            else
            {
                int valueToSet = enable ? 2 : 0;
                key.SetValue("DevicePasswordLessBuildVersion", valueToSet, RegistryValueKind.DWord);
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Windows Hello Passwordless Mode wurde auf {State} ({Value}) gesetzt.", enable ? "Aktiv" : "Inaktiv", valueToSet);
                }
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new UnauthorizedAccessException("Fehlende Rechte zum Ã„ndern des PasswordLess Registry-Keys. Programm muss als Administrator ausgefÃ¼hrt werden.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Unerwarteter Fehler beim Setzen des PasswordLess Registry-Keys.", ex);
        }
    }

    public bool IsAutoLogonEnabled()
    {
        using var key = Registry.LocalMachine.OpenSubKey(WinLogonPath, false);

        var val = key?.GetValue("AutoAdminLogon") as string;

        return val == "1";
    }

    [LibraryImport("advapi32.dll")]
    private static partial uint LsaOpenPolicy(ref IntPtr SystemName, ref LsaObjectAttributes ObjectAttributes, uint DesiredAccess, out IntPtr PolicyHandle);

    [LibraryImport("advapi32.dll", EntryPoint = "LsaStorePrivateData")]
    private static partial uint LsaStorePrivateData(IntPtr PolicyHandle, ref LsaUnicodeString KeyName, ref LsaUnicodeString PrivateData);

    // Ãœberladung zum LÃ¶schen (IntPtr fÃ¼r null)
    [LibraryImport("advapi32.dll", EntryPoint = "LsaStorePrivateData")]
    private static partial uint LsaStorePrivateData(IntPtr PolicyHandle, ref LsaUnicodeString KeyName, IntPtr PrivateData);

    [LibraryImport("advapi32.dll")]
    private static partial uint LsaClose(IntPtr ObjectHandle);

    private static void SetLsaSecret(string keyName, string? value)
    {
        var objectAttributes = new LsaObjectAttributes(); // Structs initialisieren standardmÃ¤ÃŸig auf 0/Null

        var localsystem = IntPtr.Zero;
        var secretKey = InitLsaString(keyName);
        var secretValue = value is null ? new LsaUnicodeString() : InitLsaString(value); // Leeres Struct fÃ¼r LÃ¶schung

        var lsaPolicyHandle = IntPtr.Zero;

        const uint access = 0x00000020 | 0x00000800; // POLICY_CREATE_SECRET | POLICY_LOOKUP_NAMES

        var result = LsaOpenPolicy(ref localsystem, ref objectAttributes, access, out lsaPolicyHandle);

        if (result == 0) // STATUS_SUCCESS
        {
            try
            {
                if (value is null)
                {
                    // Zum LÃ¶schen nutzen wir die Ãœberladung mit IntPtr.Zero oder Ã¼bergeben NULL je nach Definition.
                    // P/Invoke Trick: Wir nutzen hier eine zweite Definition oder IntPtr.Zero fÃ¼r den Value
                    result = LsaStorePrivateData(lsaPolicyHandle, ref secretKey, IntPtr.Zero);
                }
                else
                {
                    result = LsaStorePrivateData(lsaPolicyHandle, ref secretKey, ref secretValue);
                }

                if (result != 0)
                {
                    throw new InvalidOperationException($"LsaStorePrivateData Fehlercode: {result}");
                }

            }
            finally
            {
                _ = LsaClose(lsaPolicyHandle);

                // Speicher freigeben
                FreeLsaString(secretKey);

                if (value is not null) { FreeLsaString(secretValue); }
            }
        }
        else
        {
            throw new InvalidOperationException($"LsaOpenPolicy Fehlercode: {result}");
        }
    }

    private static LsaUnicodeString InitLsaString(string s)
    {
        // Sauberere Implementierung mit Marshal
        return new LsaUnicodeString
        {
            Length = (ushort)(s.Length * 2),
            MaximumLength = (ushort)((s.Length + 1) * 2),
            Buffer = Marshal.StringToHGlobalUni(s)
        };
    }

    private static void FreeLsaString(LsaUnicodeString lus)
    {
        if (lus.Buffer != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(lus.Buffer);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LsaUnicodeString
    {
        public UInt16 Length;
        public UInt16 MaximumLength;
        public IntPtr Buffer;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LsaObjectAttributes
    {
        public int Length;
        public IntPtr RootDirectory;
        public IntPtr ObjectName;
        public uint Attributes;
        public IntPtr SecurityDescriptor;
        public IntPtr SecurityQualityOfService;
    }

}

