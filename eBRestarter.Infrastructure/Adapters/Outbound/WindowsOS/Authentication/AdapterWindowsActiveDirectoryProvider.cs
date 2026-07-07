using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Ports.Outbound.Authentication;
using System.DirectoryServices.AccountManagement;

namespace eBRestarter.Infrastructure.Adapters.Outbound.WindowsOS.Authentication;

/// <summary>
/// Adapter: Driven Adapter (Outbound Provider) wrapping Windows Active Directory / PrincipalContext credential validation.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter / Provider)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Dienstleister im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core (Kapselung der nativen Windows-AD- und PrincipalContext-APIs).<br/>
/// - <strong>Implementierter Port:</strong> <see cref="IOutboundPortActiveDirectoryProvider"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein vorbildlicher <strong>Outbound Adapter</strong> (Taxonomie: Provider Adapter), da sie im Infrastructure-Layer liegt, einen Outbound Port implementiert und vom Core angetrieben wird, um native OS-Seiteneffekte und Domain-Prüfungen auszuführen.
/// </para>
/// </summary>
public sealed class AdapterWindowsActiveDirectoryProvider : IOutboundPortActiveDirectoryProvider
{
    public bool ValidateCredentials(DirectoryContextScope contextScope, string domain, string username, string password)
    {
        var contextType = contextScope switch
        {
            DirectoryContextScope.Domain => ContextType.Domain,
            DirectoryContextScope.ApplicationDirectory => ContextType.ApplicationDirectory,
            _ => ContextType.Machine
        };

        using var context = new PrincipalContext(contextType, domain);

        return context.ValidateCredentials(username, password);
    }
}
