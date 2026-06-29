using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Core.Application.Ports.Outbound.Network;
using System.Net.NetworkInformation;

namespace eBRestarter.Infrastructure.Adapters.WindowsOS;

public sealed class WindowsNetworkProviderAdapter : INetworkProviderPort
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



