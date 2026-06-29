using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;

public interface INetworkInfoPort
{
    bool IsNetworkAvailable();
    IEnumerable<NetworkStats> RetrieveActiveInterfaces();
}



