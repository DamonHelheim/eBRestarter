using eBRestarter.Core.Domain.Models.Records;

namespace eBRestarter.Core.Application.Interfaces;

public interface IEVisitorApiService
{
    // Holt IP Daten
    Task<IpInfoData?> GetIpInfoAsync();

    // Holt Verdienste (Username/Key kommen aus der Config, die der Service kennt)
    Task<EarningsData?> GetEarningsAsync();
}
