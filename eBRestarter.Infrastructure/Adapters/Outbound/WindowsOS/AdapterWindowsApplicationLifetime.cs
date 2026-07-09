using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Application;

namespace eBRestarter.Infrastructure.Adapters.Outbound.WindowsOS;

/// <summary>
/// Adapter: Driven Adapter (Outbound Handler/Adapter) managing Windows application shutdown and restart lifecycle.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Baustein im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core zur Beendigung oder zum Neustart des Anwendungsprozesses.<br/>
/// - <strong>Implementierter Port:</strong> <see cref="IOutboundPortApplicationLifetime"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein <strong>Outbound Adapter</strong>, da sie im Infrastructure-Layer liegt und einen Port implementiert, um OS-Lebenszyklus-Seiteneffekte auszuführen.
/// </para>
/// </summary>
public sealed class AdapterWindowsApplicationLifetime : IOutboundPortApplicationLifetime
{
    public void ExitApplication(int exitCode)
    {
        Environment.Exit(exitCode);
    }
}
