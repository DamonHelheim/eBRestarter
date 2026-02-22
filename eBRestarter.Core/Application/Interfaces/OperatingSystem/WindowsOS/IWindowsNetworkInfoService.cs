using eBRestarter.Core.Domain.Models.Records;

namespace eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;

public interface IWindowsNetworkInfoService
{
    bool IsNetworkAvailable();
    IEnumerable<NetworkStats> GetActiveInterfaces();
}
