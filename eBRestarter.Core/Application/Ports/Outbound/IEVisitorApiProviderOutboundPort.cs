using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Core.Application.Ports.Outbound;

public interface IEVisitorApiProviderOutboundPort
{
    /// <summary>
    /// Retrieves IP information data.
    /// </summary>
    Task<IpInfoData?> RetrieveIpInfoAsync();

    /// <summary>
    /// Retrieves earnings data (username/key are retrieved from the configuration known by the service).
    /// </summary>
    Task<EarningsData?> RetrieveEarningsAsync();
}
