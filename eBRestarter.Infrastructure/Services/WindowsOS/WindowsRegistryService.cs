using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using Microsoft.Win32;
using System.Runtime.Versioning;

namespace eBRestarter.Infrastructure.Services.WindowsOS;

[SupportedOSPlatform("windows")]
public class WindowsRegistryService : IWindowsRegistryService
{
    public void SetCurrentUserValue(string subKey, string name, object value)
    {
        using var key = Registry.CurrentUser.CreateSubKey(subKey);

        key.SetValue(name, value);
    }

    public void DeleteCurrentUserValue(string subKey, string name)
    {
        using var key = Registry.CurrentUser.OpenSubKey(subKey, true);

        key?.DeleteValue(name, throwOnMissingValue: false);
    }

    public void SetLocalMachineValue(string subKey, string name, object value, RegistryValueKind kind)
    {
        using var key = Registry.LocalMachine.CreateSubKey(subKey);

        key.SetValue(name, value, kind);
    }

    public Dictionary<string, object> RetrieveCurrentUserValues(string subKey)
    {
        var result = new Dictionary<string, object>();

        using var key = Registry.CurrentUser.OpenSubKey(subKey);

        if (key != null)
        {
            foreach (string name in key.GetValueNames())
            {
                var val = key.GetValue(name);
                if (val != null) result[name] = val;
            }
        }
        return result;
    }

    public object? RetrieveCurrentUserValue(string subKey, string valueName)
    {
        // OpenSubKey(..., false) bedeutet: Nur lesend öffnen (sicherer)
        using var key = Registry.CurrentUser.OpenSubKey(subKey, false);

        return key?.GetValue(valueName);
    }

    public object? RetrieveLocalMachineValue(string subKey, string valueName)
    {
        using var key = Registry.LocalMachine.OpenSubKey(subKey, false);

        return key?.GetValue(valueName);
    }
}
