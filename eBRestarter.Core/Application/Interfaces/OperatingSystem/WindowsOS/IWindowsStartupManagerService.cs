namespace eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;

public interface IWindowsStartupManagerService
{
    Task EnableAutoStartAsync();
    Task DisableAutoStartAsync();
    Task<bool> IsAutoStartEnabledAsync();

    void EnableAutoStart();
    void DisableAutoStart();
    Dictionary<string, object> GetStartupEntries();
    // Edge & AutoLogon Logik passt hier gut rein oder in einen "SystemConfigService"
    void SetEdgeStartupBoost(bool enable);

    bool IsEdgeStartupBoostEnabled();
    void SetAutoLogon(bool enable);
}
