using System.Threading.Tasks;
namespace eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
public interface IAutoStartPort
{
    Task EnableAutoStartAsync();
    Task DisableAutoStartAsync();
    Task<bool> IsAutoStartEnabledAsync();
    void EnableAutoStart();
    void DisableAutoStart();
    System.Collections.Generic.Dictionary<string, object> RetrieveStartupEntries();
}
