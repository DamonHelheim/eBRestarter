using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace eBRestarter.Infrastructure.Services.WindowsOS.Security;

[SupportedOSPlatform("windows")]
public class WindowsAutoLogonService(ILogger<WindowsAutoLogonService> logger) : IWindowsAutoLogonService
{
    private const string WinLogonPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon";
    private const string DefaultPasswordKey = "DefaultPassword";
    private readonly ILogger<WindowsAutoLogonService> _logger = logger;

    public void EnableAutoLogon(string username, string domain, string password)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(WinLogonPath, true);
            if (key == null) throw new InvalidOperationException("Winlogon Registry Key nicht gefunden.");

            // 1. Registry Werte setzen
            key.SetValue("AutoAdminLogon", "1", RegistryValueKind.String);
            key.SetValue("DefaultUserName", username, RegistryValueKind.String);

            if (!string.IsNullOrEmpty(domain))
                key.SetValue("DefaultDomainName", domain, RegistryValueKind.String);
            else
                key.DeleteValue("DefaultDomainName", false);

            // WICHTIG: Klartext-Passwort aus Registry löschen, falls vorhanden
            key.DeleteValue(DefaultPasswordKey, false);

            // 2. Passwort sicher in LSA Secrets speichern
            SetLsaSecret(DefaultPasswordKey, password);

            _logger.LogInformation("AutoLogon für User '{User}' erfolgreich aktiviert.", username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Aktivieren von AutoLogon.");
            throw;
        }
    }

    public void DisableAutoLogon()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(WinLogonPath, true);
            key?.SetValue("AutoAdminLogon", "0", RegistryValueKind.String);

            // LSA Secret löschen
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

            if (key != null)
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

    public bool IsAutoLogonEnabled()
    {
        using var key = Registry.LocalMachine.OpenSubKey(WinLogonPath, false);
        var val = key?.GetValue("AutoAdminLogon") as string;
        return val == "1";
    }

    // --- Private Helper & P/Invoke (Interne Logik) ---

    private void SetLsaSecret(string keyName, string? value)
    {
        var objectAttributes = new LSA_OBJECT_ATTRIBUTES(); // Structs initialisieren standardmäßig auf 0/Null

        var localsystem = IntPtr.Zero;
        var secretKey = InitLsaString(keyName);
        var secretValue = (value != null) ? InitLsaString(value) : new LSA_UNICODE_STRING(); // Leeres Struct für Löschung

        IntPtr lsaPolicyHandle = IntPtr.Zero;
        uint access = 0x00000020 | 0x00000800; // POLICY_CREATE_SECRET | POLICY_LOOKUP_NAMES

        var result = LsaOpenPolicy(ref localsystem, ref objectAttributes, access, out lsaPolicyHandle);

        if (result == 0) // STATUS_SUCCESS
        {
            try
            {
                if (value != null)
                {
                    result = LsaStorePrivateData(lsaPolicyHandle, ref secretKey, ref secretValue);
                }
                else
                {
                    // Zum Löschen nutzen wir die Überladung mit IntPtr.Zero oder übergeben NULL je nach Definition.
                    // P/Invoke Trick: Wir nutzen hier eine zweite Definition oder IntPtr.Zero für den Value
                    result = LsaStorePrivateData(lsaPolicyHandle, ref secretKey, IntPtr.Zero);
                }

                if (result != 0)
                    throw new Exception($"LsaStorePrivateData Fehlercode: {result}");
            }
            finally
            {
                LsaClose(lsaPolicyHandle);
                // Speicher freigeben
                FreeLsaString(secretKey);
                if (value != null) FreeLsaString(secretValue);
            }
        }
        else
        {
            throw new Exception($"LsaOpenPolicy Fehlercode: {result}");
        }
    }

    private LSA_UNICODE_STRING InitLsaString(string s)
    {
        // Sauberere Implementierung mit Marshal
        return new LSA_UNICODE_STRING
        {
            Length = (ushort)(s.Length * 2),
            MaximumLength = (ushort)((s.Length + 1) * 2),
            Buffer = Marshal.StringToHGlobalUni(s)
        };
    }

    private void FreeLsaString(LSA_UNICODE_STRING lus)
    {
        if (lus.Buffer != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(lus.Buffer);
        }
    }

    // --- P/Invoke Definitionen (müssen static extern sein) ---

    [StructLayout(LayoutKind.Sequential)]
    private struct LSA_UNICODE_STRING
    {
        public UInt16 Length;
        public UInt16 MaximumLength;
        public IntPtr Buffer;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LSA_OBJECT_ATTRIBUTES
    {
        public int Length;
        public IntPtr RootDirectory;
        public IntPtr ObjectName;
        public uint Attributes;
        public IntPtr SecurityDescriptor;
        public IntPtr SecurityQualityOfService;
    }

    [DllImport("advapi32.dll")]
    private static extern uint LsaOpenPolicy(ref IntPtr SystemName, ref LSA_OBJECT_ATTRIBUTES ObjectAttributes, uint DesiredAccess, out IntPtr PolicyHandle);

    [DllImport("advapi32.dll", EntryPoint = "LsaStorePrivateData")]
    private static extern uint LsaStorePrivateData(IntPtr PolicyHandle, ref LSA_UNICODE_STRING KeyName, ref LSA_UNICODE_STRING PrivateData);

    // Überladung zum Löschen (IntPtr für null)
    [DllImport("advapi32.dll", EntryPoint = "LsaStorePrivateData")]
    private static extern uint LsaStorePrivateData(IntPtr PolicyHandle, ref LSA_UNICODE_STRING KeyName, IntPtr PrivateData);

    [DllImport("advapi32.dll")]
    private static extern uint LsaClose(IntPtr ObjectHandle);
}
