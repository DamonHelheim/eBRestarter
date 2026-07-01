using System.Net.NetworkInformation;

namespace eBRestarter.Core.Application.Ports.Outbound.Network;

// Abstrahiert die statischen Systemaufrufe
public interface INetworkProviderOutboundPort
{
    bool CheckIsNetworkAvailable();
    NetworkInterface[] RetrieveAllNetworkInterfaces();
}


