using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Network;
using System.Net.NetworkInformation;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Provider.WindowsOS;

/// <summary>
/// Adapter: Driven Adapter (Outbound Provider) for checking Windows network connectivity via NetworkInterface.
/// <para>
/// <strong>Architecture Classification: OUTBOUND ADAPTER (Driven Adapter / Provider)</strong><br/>
/// - <strong>Role &amp; Responsibility:</strong> Queries OS network interface status and connectivity in the Infrastructure layer.<br/>
/// - <strong>Implemented Port:</strong> <see cref="IOutboundPortNetworkProvider"/>.<br/>
/// </para>
/// </summary>
public sealed class AdapterWindowsNetworkProvider : IOutboundPortNetworkProvider
{
    /// <summary>
    /// Determines whether any network interface is available and connected to a network.
    /// </summary>
    public bool CheckIsNetworkAvailable()
    {
        return NetworkInterface.GetIsNetworkAvailable();
    }

    /// <summary>
    /// Retrieves objects that describe all network interfaces on the local computer.
    /// </summary>
    public NetworkInterface[] RetrieveAllNetworkInterfaces()
    {
        return NetworkInterface.GetAllNetworkInterfaces();
    }
}



