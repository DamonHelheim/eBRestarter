using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Network;
using System.Net.NetworkInformation;

namespace eBRestarter.Infrastructure.BehavioralComponents.Providers;

public sealed class NetworkInfoProvider(IOutboundPortNetworkProvider networkProvider) : IInboundPortNetworkInfoProvider
{
    private readonly IOutboundPortNetworkProvider _networkProvider = networkProvider;

    public bool IsNetworkAvailable() => _networkProvider.CheckIsNetworkAvailable();

    public IEnumerable<NetworkStats> RetrieveActiveInterfaces()
    {
        var interfaces = _networkProvider.RetrieveAllNetworkInterfaces();

        foreach (var nic in interfaces)
        {
            // Filter: Process only active network adapters and ignore loopback interfaces
            if (nic.OperationalStatus != OperationalStatus.Up || nic.NetworkInterfaceType == NetworkInterfaceType.Loopback)
            {
                continue;
            }

            var stats = nic.GetIPv4Statistics();

            // Filter out adapters that have recorded zero network traffic activity
            if (stats.BytesSent == 0 && stats.BytesReceived == 0)
            {
                continue;
            }

            yield return new NetworkStats(
                Name: nic.Name,
                BytesReceived: stats.BytesReceived,
                BytesSent: stats.BytesSent,
                IsActive: true
            );
        }
    }
}






