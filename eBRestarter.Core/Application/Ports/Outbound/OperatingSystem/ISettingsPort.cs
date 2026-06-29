namespace eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;

public interface ISettingsPort
{
    void SetUserValue(string subKey, string name, object value);
    void DeleteUserValue(string subKey, string name);
    void SetSystemValue(string subKey, string name, object value);
    object? GetUserValue(string subKey, string valueName);
    Dictionary<string, object> GetUserValues(string subKey);
    object? GetSystemValue(string subKey, string valueName);
}
