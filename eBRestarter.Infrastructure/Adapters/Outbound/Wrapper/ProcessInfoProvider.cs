using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;

namespace eBRestarter.Infrastructure.Adapters.Wrapper;

/// <summary>
/// Adapter: Driven Adapter (Outbound Provider) for retrieving current running process information.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter / Provider)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Baustein im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core zur Abfrage von Prozess-Metadaten.<br/>
/// - <strong>Implementierter Port:</strong> <see cref="IProcessInfoProviderOutboundPort"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein vorbildlicher <strong>Outbound Adapter</strong>, da sie im Infrastructure-Layer liegt, einen Outbound Port implementiert und vom Core angetrieben wird, um Systemabfragen auszuführen.
/// </para>
/// </summary>
public sealed class ProcessInfoProvider : IProcessInfoProviderOutboundPort
{
    public ProcessInfoProvider()
    {
    }

    public string GetCurrentExecutablePath()
    {
        return Environment.ProcessPath!;
    }
}


