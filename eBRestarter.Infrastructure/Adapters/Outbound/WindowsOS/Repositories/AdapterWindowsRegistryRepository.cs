using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using Microsoft.Win32;
using System.Runtime.Versioning;

namespace eBRestarter.Infrastructure.Adapters.Outbound.WindowsOS.Repositories;

/// <summary>
/// Adapter: Driven Adapter (Outbound Repository/Adapter) managing persistence of user settings in Windows Registry.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter / Repository)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Baustein im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core durch Lese- und Schreibzugriffe auf die Windows Registry (<see cref="Registry.CurrentUser"/>).<br/>
/// - <strong>Implementierter Port:</strong> <see cref="IOutboundPortSystemConfigurationRepository"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein vorbildlicher <strong>Outbound Adapter</strong>, da sie im Infrastructure-Layer liegt, einen Outbound Port implementiert und vom Core angetrieben wird, um Einstellungen zu persistieren.
/// </para>
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class AdapterWindowsRegistryRepository : IOutboundPortSystemConfigurationRepository
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