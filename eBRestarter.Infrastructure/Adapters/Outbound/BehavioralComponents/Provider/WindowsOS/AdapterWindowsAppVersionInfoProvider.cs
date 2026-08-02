using System.Reflection;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Application;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Provider.WindowsOS;

/// <summary>
/// Adapter: Driven Adapter (Outbound Provider) for reading Windows executable assembly version and build metadata.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter / Provider)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Baustein im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core zur Ermittlung von Versionsinformationen aus Dateisystem und Assembly.<br/>
/// - <strong>Implementierter Port:</strong> <see cref="IOutboundPortAppVersionInfoProvider"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein vorbildlicher <strong>Outbound Adapter</strong> (Taxonomie: Provider Adapter), da sie im Infrastructure-Layer liegt, einen Outbound Port implementiert und vom Core angetrieben wird, um technologische Metadaten-Abfragen auszuführen.
/// </para>
/// </summary>
public sealed class AdapterWindowsAppVersionInfoProvider : IOutboundPortAppVersionInfoProvider
{
    private static readonly string FallbackVersion = new Version(1, 0, 0, 0).ToString();

    public string RetrieveAppVersion() =>
        Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? FallbackVersion;
}
