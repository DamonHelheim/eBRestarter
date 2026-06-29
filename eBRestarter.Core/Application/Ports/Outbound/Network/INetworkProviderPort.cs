using System.Net.NetworkInformation;

namespace eBRestarter.Core.Application.Ports.Outbound.Network;

// Abstrahiert die statischen Systemaufrufe
public interface INetworkProviderPort
{
    bool CheckIsNetworkAvailable();
    NetworkInterface[] RetrieveAllNetworkInterfaces();
}


