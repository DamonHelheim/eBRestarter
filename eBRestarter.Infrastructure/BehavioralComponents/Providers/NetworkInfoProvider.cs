using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;

using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Network;

namespace eBRestarter.Infrastructure.BehavioralComponents.Providers;

/// <summary>
/// Provider Component: Infrastructure provider aggregating and filtering system network interfaces.
/// </summary>
/// <param name="networkProvider">The outbound port network provider retrieving low-level network interface statistics.</param>
public sealed class NetworkInfoProvider(
    IOutboundPortNetworkProvider networkProvider)
    : IInboundPortNetworkInfoProvider
{
    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════

    // ── Block 1: Injected Dependencies (alphabetical) ──
    private readonly IOutboundPortNetworkProvider _networkProvider = networkProvider ?? throw new ArgumentNullException(nameof(networkProvider));


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════

    /// <inheritdoc />
    public bool IsNetworkAvailable() => _networkProvider.CheckIsNetworkAvailable();

    /// <inheritdoc />
    public IEnumerable<NetworkStats> RetrieveActiveInterfaces()
    {
        var interfaces = _networkProvider.RetrieveAllNetworkInterfaces();

        foreach (var networkInterface in interfaces)
        {
            if (networkInterface.OperationalStatus != OperationalStatus.Up || networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
            {
                continue;
            }

            var ipv4Statistics = networkInterface.GetIPv4Statistics();

            if (ipv4Statistics.BytesSent == 0 && ipv4Statistics.BytesReceived == 0)
            {
                continue;
            }

            yield return new NetworkStats(
                Name: networkInterface.Name,
                BytesReceived: ipv4Statistics.BytesReceived,
                BytesSent: ipv4Statistics.BytesSent,
                IsActive: true);
        }
    }
}
