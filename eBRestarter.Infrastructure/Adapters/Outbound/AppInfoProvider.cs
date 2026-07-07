using eBRestarter.Core.Application.Ports.Outbound.Application;
using System.Diagnostics;
using System.Reflection;

namespace eBRestarter.Infrastructure.Adapters;

/// <summary>
/// Adapter: Driven Adapter (Outbound Provider) for retrieving application version and runtime metadata.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter / Provider)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Dienstleister im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core (Abfrage von schreibgeschützten System- und Assembly-Werten via Reflection).<br/>
/// - <strong>Implementierter Port:</strong> <see cref="IAppInfoProviderOutboundPort"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein vorbildlicher <strong>Outbound Adapter</strong> (Taxonomie: Provider Adapter), da sie im Infrastructure-Layer liegt, einen Outbound Port implementiert und vom Core angetrieben wird, um technologische Seiteneffekte bzw. OS-/Assembly-Abfragen auszuführen.
/// </para>
/// </summary>
public sealed class AppInfoProvider : IAppInfoProviderOutboundPort
{
    public string RetrieveAppVersion()
    {
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
        return fvi.FileVersion ?? "1.0.0";
    }
}





