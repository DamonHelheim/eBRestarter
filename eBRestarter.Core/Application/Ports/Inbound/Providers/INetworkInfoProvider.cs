using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Core.Application.Ports.Inbound.Providers;

public interface INetworkInfoProvider
{
    bool IsNetworkAvailable();
    IEnumerable<NetworkStats> RetrieveActiveInterfaces();
}
