namespace eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
public interface IBrowserConfigRepositoryOutboundPort
{
    void SetBrowserStartupBoost(bool enable);
    bool IsBrowserStartupBoostEnabled();
}
