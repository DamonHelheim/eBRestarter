using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using Microsoft.Win32;
using System.Runtime.Versioning;
using System.Collections.Generic;

namespace eBRestarter.Infrastructure.Adapters.WindowsOS;

[SupportedOSPlatform("windows")]
public sealed class WindowsRegistryRepository : ISettingsPort
{
    public void SetUserValue(string subKey, string name, object value)
    {
        using var key = Registry.CurrentUser.CreateSubKey(subKey);
        key.SetValue(name, value);
    }

    public void DeleteUserValue(string subKey, string name)
    {
        using var key = Registry.CurrentUser.OpenSubKey(subKey, true);
        key?.DeleteValue(name, throwOnMissingValue: false);
    }

    public void SetSystemValue(string subKey, string name, object value)
    {
        using var key = Registry.LocalMachine.CreateSubKey(subKey);
        // Default to String/DWord automatically
        key.SetValue(name, value);
    }

    public Dictionary<string, object> GetUserValues(string subKey)
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

    public object? GetUserValue(string subKey, string valueName)
    {
        using var key = Registry.CurrentUser.OpenSubKey(subKey, false);
        return key?.GetValue(valueName);
    }

    public object? GetSystemValue(string subKey, string valueName)
    {
        using var key = Registry.LocalMachine.OpenSubKey(subKey, false);
        return key?.GetValue(valueName);
    }
}