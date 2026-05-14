using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Core.Application.Interfaces;

public interface IEVisitorApiService
{
    // Holt IP Daten
    Task<IpInfoData?> RetrieveIpInfoAsync();

    // Holt Verdienste (Username/Key kommen aus der Config, die der Service kennt)
    Task<EarningsData?> RetrieveEarningsAsync();
}
