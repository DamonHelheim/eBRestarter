using eBRestarter.Core.Domain.Models.Records;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Application.Interfaces
{
    public interface IEVisitorApiService
    {
        // Holt IP Daten
        Task<IpInfoData?> GetIpInfoAsync();

        // Holt Verdienste (Username/Key kommen aus der Config, die der Service kennt)
        Task<EarningsData?> GetEarningsAsync();
    }
}
