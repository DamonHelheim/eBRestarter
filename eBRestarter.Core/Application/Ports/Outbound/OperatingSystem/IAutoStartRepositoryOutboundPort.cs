namespace eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;

public interface IAutoStartRepositoryOutboundPort
{
    Task EnableAutoStartAsync();
    Task DisableAutoStartAsync();
    Task<bool> IsAutoStartEnabledAsync();
    void EnableAutoStart();
    void DisableAutoStart();
    Dictionary<string, object> RetrieveStartupEntries();
}
