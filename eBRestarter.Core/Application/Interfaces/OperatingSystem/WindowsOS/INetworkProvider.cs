using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS
{
    // Abstrahiert die statischen Systemaufrufe
    public interface INetworkProvider
    {
        bool GetIsNetworkAvailable();
        NetworkInterface[] GetAllNetworkInterfaces();
    }
}
