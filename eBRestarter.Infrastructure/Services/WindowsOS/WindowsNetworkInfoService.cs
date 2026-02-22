using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Domain.Models.Records;
using System.Net.NetworkInformation;

namespace eBRestarter.Infrastructure.Services.WindowsOS;

public class WindowsNetworkInfoService : IWindowsNetworkInfoService
{
    public bool IsNetworkAvailable() => NetworkInterface.GetIsNetworkAvailable();

    public IEnumerable<NetworkStats> GetActiveInterfaces()
    {
        var interfaces = NetworkInterface.GetAllNetworkInterfaces();

        foreach (var nic in interfaces)
        {
            // Filter: Nur aktive Karten und keine Loopbacks
            if (nic.OperationalStatus != OperationalStatus.Up || nic.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                continue;

            var stats = nic.GetIPv4Statistics();

            // Wir filtern Karten ohne Traffic, wie in deinem alten Code
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
