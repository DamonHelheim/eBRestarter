using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Application.Models.Records;
using System.Net.NetworkInformation;

namespace eBRestarter.Infrastructure.Services.WindowsOS;

public class WindowsNetworkInfoAdapter(INetworkProvider WindowsNetworkAdapter) : IWindowsNetworkInfoUseCase
{
    private readonly INetworkProvider _networkProvider = WindowsNetworkAdapter;

    public bool IsNetworkAvailable() => _networkProvider.CheckIsNetworkAvailable();

    public IEnumerable<NetworkStats> RetrieveActiveInterfaces()
    {
        var interfaces = _networkProvider.RetrieveAllNetworkInterfaces();

        foreach (var nic in interfaces)
        {
            // Filter: Nur aktive Karten und keine Loopbacks
            if (nic.OperationalStatus != OperationalStatus.Up || nic.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                continue;

            var stats = nic.GetIPv4Statistics();

            // Wir filtern Karten ohne Traffic
            if (stats.BytesSent == 0 && stats.BytesReceived == 0)
                continue;

            yield return new NetworkStats(
                Name: nic.Name,
                BytesReceived: stats.BytesReceived,
                BytesSent: stats.BytesSent,
                IsActive: true
            );
        }
    }
}


