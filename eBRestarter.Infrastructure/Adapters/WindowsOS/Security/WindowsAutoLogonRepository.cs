using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace eBRestarter.Infrastructure.Adapters.WindowsOS.Security;

[SupportedOSPlatform("windows")]
public sealed partial class WindowsAutoLogonRepository(ILogger<WindowsAutoLogonRepository> logger) : IOsAutoLogonPort
{
    private const string WinLogonPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon";
    private const string DefaultPasswordKey = "DefaultPassword";

    private readonly ILogger<WindowsAutoLogonRepository> _logger = logger;

    public void EnableAutoLogon(string username, string domain, string password)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(WinLogonPath, true) ?? throw new InvalidOperationException("Winlogon Registry Key not found.");

            // 1. Set standard registry parameters
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

            // IMPORTANT: Remove the plain-text password entry from the registry if it exists
            key.DeleteValue(DefaultPasswordKey, false);

            // 2. Persist the password securely inside LSA Secrets
            SetLsaSecret(DefaultPasswordKey, password);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("AutoLogon successfully enabled for user '{User}'.", username);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to enable AutoLogon.", ex);
        }
    }

    public void DisableAutoLogon()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(WinLogonPath, true);

            key?.SetValue("AutoAdminLogon", "0", RegistryValueKind.String);

            // Purge the password from LSA Secrets
            SetLsaSecret(DefaultPasswordKey, null);

            _logger.LogInformation("AutoLogon disabled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disable AutoLogon.");
        }
    }

    public bool IsPasswordlessAuthEnabled()
    {
        try
        {
            // The exact path where Windows 11 stores the "Allow Windows Hello sign-in only" configuration toggle
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\PasswordLess\Device", false);

            if (key is not null)
            {
                var val = key.GetValue("DevicePasswordLessBuildVersion");

                // If the value is '2', the user has enabled the option (blocking standard password login attempts).
                // If the value is '0' (or does not exist), the passwordless option is disabled.
                if (val is int intValue && intValue == 2)
                {
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while reading the PasswordLess registry key.");
        }

        return false;
    }

    public void SetPasswordlessAuth(bool enable)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\PasswordLess\Device", true);

            if (key is null)
            {
                _logger.LogWarning("Registry key for PasswordLess Device not found. Skipping recreation since this is system-specific.");
            }
            else
            {
                int valueToSet = enable ? 2 : 0;
                key.SetValue("DevicePasswordLessBuildVersion", valueToSet, RegistryValueKind.DWord);

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Windows Hello Passwordless Mode changed to {State} ({Value}).", enable ? "Active" : "Inactive", valueToSet);
                }
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new UnauthorizedAccessException("Insufficient permissions to modify the PasswordLess registry key. The application must be run as Administrator.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An unexpected error occurred while writing the PasswordLess registry key.", ex);
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

    // Overload variant used for deletion (IntPtr for null values)
    [LibraryImport("advapi32.dll", EntryPoint = "LsaStorePrivateData")]
    private static partial uint LsaStorePrivateData(IntPtr PolicyHandle, ref LsaUnicodeString KeyName, IntPtr PrivateData);

    [LibraryImport("advapi32.dll")]
    private static partial uint LsaClose(IntPtr ObjectHandle);

    private static void SetLsaSecret(string keyName, string? value)
    {
        var objectAttributes = new LsaObjectAttributes(); // Struct elements initialize to 0/Null by default

        var localsystem = IntPtr.Zero;
        var secretKey = InitLsaString(keyName);
        var secretValue = value is null ? new LsaUnicodeString() : InitLsaString(value); // Empty struct configuration handles deletion requests

        var lsaPolicyHandle = IntPtr.Zero;

        const uint access = 0x00000020 | 0x00000800; // POLICY_CREATE_SECRET | POLICY_LOOKUP_NAMES

        var result = LsaOpenPolicy(ref localsystem, ref objectAttributes, access, out lsaPolicyHandle);

        if (result == 0) // STATUS_SUCCESS
        {
            try
            {
                if (value is null)
                {
                    // For deletions we rely on the specific method signature variant feeding an explicit IntPtr.Zero
                    result = LsaStorePrivateData(lsaPolicyHandle, ref secretKey, IntPtr.Zero);
                }
                else
                {
                    result = LsaStorePrivateData(lsaPolicyHandle, ref secretKey, ref secretValue);
                }

                if (result != 0)
                {
                    throw new InvalidOperationException($"LsaStorePrivateData error code: {result}");
                }
            }
            finally
            {
                _ = LsaClose(lsaPolicyHandle);

                // Reclaim allocated native memory
                FreeLsaString(secretKey);

                if (value is not null) { FreeLsaString(secretValue); }
            }
        }
        else
        {
            throw new InvalidOperationException($"LsaOpenPolicy error code: {result}");
        }
    }

    private static LsaUnicodeString InitLsaString(string s)
    {
        // Safe marshaling native memory layout conversion approach
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