using eBRestarter.Core.Domain.Models.Records;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS
{
    public interface IWindowsNetworkInfoService
    {
        bool IsNetworkAvailable();
        IEnumerable<NetworkStats> GetActiveInterfaces();
    }
}
