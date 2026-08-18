using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;

using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Repositories.WindowsOS;

/// <summary>
/// Adapter: Driven Adapter (Outbound Repository/Adapter) managing persistence of user settings in Windows Registry.
/// <para>
/// <strong>Architecture Classification: OUTBOUND ADAPTER (Driven Adapter / Repository)</strong><br/>
/// - <strong>Role &amp; Responsibility:</strong> Reads and writes configuration settings in the Windows Registry (<see cref="Registry.CurrentUser"/> and <see cref="Registry.LocalMachine"/>) in the Infrastructure layer.<br/>
/// - <strong>Implemented Port:</strong> <see cref="IOutboundPortSystemConfigurationRepository"/>.<br/>
/// </para>
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class AdapterWindowsRegistryRepository : IOutboundPortSystemConfigurationRepository
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitives & strings ──
    private const string FailedToCreateOrOpenSubKeyExceptionMessage = "Failed to create or open registry subkey '{0}'.";
    private const string FailedToCreateOrOpenSystemSubKeyExceptionMessage = "Failed to create or open system registry subkey '{0}'.";
    private const string InsufficientPermissionsSystemSubKeyExceptionMessage = "Insufficient permissions to write to system registry subkey '{0}'. The application must be run as Administrator.";


    // ═══════════════════════════════════════════════════════
    //  8. Methods (public → private)
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Deletes a value entry under HKEY_CURRENT_USER for the specified subkey.
    /// </summary>
    /// <param name="subKey">Registry subkey path.</param>
    /// <param name="name">Value entry name to delete.</param>
    public void DeleteUserValue(string subKey, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subKey);
        ArgumentNullException.ThrowIfNull(name);

        using var key = Registry.CurrentUser.OpenSubKey(subKey, true);
        key?.DeleteValue(name, throwOnMissingValue: false);
    }

    /// <summary>
    /// Reads a value entry under HKEY_LOCAL_MACHINE for the specified subkey.
    /// </summary>
    /// <param name="subKey">Registry subkey path.</param>
    /// <param name="valueName">Value entry name.</param>
    public object? GetSystemValue(string subKey, string valueName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subKey);
        ArgumentNullException.ThrowIfNull(valueName);

        using var key = Registry.LocalMachine.OpenSubKey(subKey, false);
        return key?.GetValue(valueName);
    }

    /// <summary>
    /// Reads a value entry under HKEY_CURRENT_USER for the specified subkey.
    /// </summary>
    /// <param name="subKey">Registry subkey path.</param>
    /// <param name="valueName">Value entry name.</param>
    public object? GetUserValue(string subKey, string valueName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subKey);
        ArgumentNullException.ThrowIfNull(valueName);

        using var key = Registry.CurrentUser.OpenSubKey(subKey, false);
        return key?.GetValue(valueName);
    }

    /// <summary>
    /// Reads all value entries under HKEY_CURRENT_USER for the specified subkey into a dictionary.
    /// </summary>
    /// <param name="subKey">Registry subkey path.</param>
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

    /// <summary>
    /// Writes a value entry under HKEY_LOCAL_MACHINE for the specified subkey.
    /// </summary>
    /// <param name="subKey">Registry subkey path.</param>
    /// <param name="name">Value entry name.</param>
    /// <param name="value">Object value to set.</param>
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

    /// <summary>
    /// Writes a value entry under HKEY_CURRENT_USER for the specified subkey.
    /// </summary>
    /// <param name="subKey">Registry subkey path.</param>
    /// <param name="name">Value entry name.</param>
    /// <param name="value">Object value to set.</param>
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