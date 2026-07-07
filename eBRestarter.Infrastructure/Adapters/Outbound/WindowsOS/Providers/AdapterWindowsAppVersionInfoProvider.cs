using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Application;
using System.Reflection;

namespace eBRestarter.Infrastructure.Adapters.Outbound.WindowsOS.Providers;

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
    public string RetrieveAppVersion()
    {
        // Since the application is now Unpackaged (classic standalone .exe distribution format),
        // we extract the version metadata attributes directly from the entry assembly context.
        var assemblyVersion = Assembly.GetEntryAssembly()?.GetName().Version;

        if (assemblyVersion is not null)
        {
            return $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}.{assemblyVersion.Revision}";
        }

        return new Version(1, 0, 0, 0).ToString(); // Fallback threshold execution
    }
}




