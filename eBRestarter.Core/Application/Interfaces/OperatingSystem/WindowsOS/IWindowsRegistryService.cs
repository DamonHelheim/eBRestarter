using Microsoft.Win32;

namespace eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;

public interface IWindowsRegistryService
{
    void SetCurrentUserValue(string subKey, string name, object value);
    void DeleteCurrentUserValue(string subKey, string name);
    void SetLocalMachineValue(string subKey, string name, object value, RegistryValueKind kind);

    /// <summary>
    /// Liest einen einzelnen Wert aus HKEY_CURRENT_USER.
    /// </summary>
    object? GetCurrentUserValue(string subKey, string valueName);

    /// <summary>
    /// Liest alle Werte eines Schlüssels aus HKEY_CURRENT_USER.
    /// </summary>
    Dictionary<string, object> GetCurrentUserValues(string subKey);

    /// <summary>
    /// Liest einen einzelnen Wert aus HKEY_LOCAL_MACHINE.
    /// </summary>
    object? GetLocalMachineValue(string subKey, string valueName);
}
