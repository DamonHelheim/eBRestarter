using eBRestarter.Core.Application.Ports.Outbound.Network;
using System.Net.NetworkInformation;

namespace eBRestarter.Infrastructure.Adapters.WindowsOS;

/// <summary>
/// Adapter: Driven Adapter (Outbound Provider) for checking Windows network connectivity via NetworkInterface.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter / Provider)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Baustein im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core durch Abfrage des OS-Netzwerkverbindungsstatus.<br/>
/// - <strong>Implementierter Port:</strong> <see cref="INetworkProviderOutboundPort"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein vorbildlicher <strong>Outbound Adapter</strong> (Taxonomie: Provider Adapter), da sie im Infrastructure-Layer liegt, einen Outbound Port implementiert und vom Core angetrieben wird, um technologische System-Abfragen auszuführen.
/// </para>
/// </summary>
public sealed class WindowsNetworkProvider : INetworkProviderOutboundPort
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



