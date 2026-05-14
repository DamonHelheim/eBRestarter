using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using System.Net.NetworkInformation;

namespace eBRestarter.Infrastructure.Services.WindowsOS;

public class NetworkProvider : INetworkProvider
{
    public bool CheckIsNetworkAvailable()
    {
        return NetworkInterface.GetIsNetworkAvailable();
    }

    public NetworkInterface[] RetrieveAllNetworkInterfaces()
    {
        return NetworkInterface.GetAllNetworkInterfaces();
    }
}
