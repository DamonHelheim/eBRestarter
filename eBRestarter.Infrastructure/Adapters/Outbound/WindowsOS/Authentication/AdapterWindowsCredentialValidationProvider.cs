using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Ports.Outbound.Authentication;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using System.DirectoryServices.AccountManagement;

namespace eBRestarter.Infrastructure.Adapters.Outbound.WindowsOS.Authentication;

/// <summary>
/// Adapter: Driven Adapter (Outbound Provider) for validating Windows and Domain credentials.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter / Provider)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Dienstleister im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core zur Validierung von Anmeldedaten.<br/>
/// - <strong>Implementierter Port:</strong> <see cref="IOutboundPortCredentialValidationProvider"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein vorbildlicher <strong>Outbound Adapter</strong> (Taxonomie: Provider Adapter), da sie im Infrastructure-Layer liegt, einen Outbound Port implementiert und vom Core angetrieben wird, um technologische OS-/ActiveDirectory-Authentifizierungen auszuführen.
/// </para>
/// </summary>
public sealed class AdapterWindowsCredentialValidationProvider(IOutboundPortActiveDirectoryProvider adProviderPort) : IOutboundPortCredentialValidationProvider
{
    private readonly IOutboundPortActiveDirectoryProvider _adProviderPort = adProviderPort;

    /// <inheritdoc />
    public bool ValidateCredentials(string username, string domain, string password)
    {
        try
        {
            var contextScope = DirectoryContextScope.Machine;

            if (!string.Equals(domain, Environment.MachineName, StringComparison.OrdinalIgnoreCase))
            {
                contextScope = DirectoryContextScope.Domain;
            }

            // We are now calling the mocked interface here instead of using 'new'!
            return _adProviderPort.ValidateCredentials(contextScope, domain, username, password);
        }
        catch (PrincipalServerDownException)
        {
            throw new InvalidOperationException("PrincipalServerDown");
        }
        catch (Exception)
        {
            return false;
        }
    }
}



