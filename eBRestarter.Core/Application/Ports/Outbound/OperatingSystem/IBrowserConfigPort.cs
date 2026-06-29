namespace eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
public interface IBrowserConfigPort
{
    void SetBrowserStartupBoost(bool enable);
    bool IsBrowserStartupBoostEnabled();
}
