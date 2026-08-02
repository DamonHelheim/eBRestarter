using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;

using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Repositories.WindowsOS;

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
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitive Typen & Strings ──
    private const string FailedToCreateOrOpenSubKeyExceptionMessage = "Failed to create or open registry subkey '{0}'.";
    private const string FailedToCreateOrOpenSystemSubKeyExceptionMessage = "Failed to create or open system registry subkey '{0}'.";
    private const string InsufficientPermissionsSystemSubKeyExceptionMessage = "Insufficient permissions to write to system registry subkey '{0}'. The application must be run as Administrator.";


    // ═══════════════════════════════════════════════════════
    //  8. Methods (public → private)
    // ═══════════════════════════════════════════════════════
    public void DeleteUserValue(string subKey, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subKey);
        ArgumentNullException.ThrowIfNull(name);

        using var key = Registry.CurrentUser.OpenSubKey(subKey, true);
        key?.DeleteValue(name, throwOnMissingValue: false);
    }

    public object? GetSystemValue(string subKey, string valueName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subKey);
        ArgumentNullException.ThrowIfNull(valueName);

        using var key = Registry.LocalMachine.OpenSubKey(subKey, false);
        return key?.GetValue(valueName);
    }

    public object? GetUserValue(string subKey, string valueName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subKey);
        ArgumentNullException.ThrowIfNull(valueName);

        using var key = Registry.CurrentUser.OpenSubKey(subKey, false);
        return key?.GetValue(valueName);
    }

    public Dictionary<string, object> GetUserValues(string subKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subKey);

        using var key = Registry.CurrentUser.OpenSubKey(subKey);
        if (key is null)
        {
            return [];
        }

        Dictionary<string, object> result = [];
        foreach (string name in key.GetValueNames())
        {
            if (key.GetValue(name) is { } val)
            {
                result[name] = val;
            }
        }

        return result;
    }

    public void SetSystemValue(string subKey, string name, object value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subKey);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(value);

        try
        {
            using var key = Registry.LocalMachine.CreateSubKey(subKey)
                ?? throw new InvalidOperationException(string.Format(FailedToCreateOrOpenSystemSubKeyExceptionMessage, subKey));

            // Default to String/DWord automatically
            key.SetValue(name, value);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new UnauthorizedAccessException(string.Format(InsufficientPermissionsSystemSubKeyExceptionMessage, subKey), ex);
        }
    }

    public void SetUserValue(string subKey, string name, object value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subKey);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(value);

        using var key = Registry.CurrentUser.CreateSubKey(subKey)
            ?? throw new InvalidOperationException(string.Format(FailedToCreateOrOpenSubKeyExceptionMessage, subKey));

        key.SetValue(name, value);
    }
}