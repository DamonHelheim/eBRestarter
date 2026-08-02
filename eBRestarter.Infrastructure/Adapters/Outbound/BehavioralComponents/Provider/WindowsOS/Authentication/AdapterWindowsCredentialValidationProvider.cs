using System.DirectoryServices.AccountManagement;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Authentication;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Provider.WindowsOS.Authentication;

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
    private readonly IOutboundPortActiveDirectoryProvider _adProviderPort = adProviderPort ?? throw new ArgumentNullException(nameof(adProviderPort));

    /// <inheritdoc />
    public bool ValidateCredentials(string username, string domain, string password)
    {
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(password);

        try
        {
            var contextScope = !string.Equals(domain, Environment.MachineName, StringComparison.OrdinalIgnoreCase)
                ? DirectoryContextScope.Domain
                : DirectoryContextScope.Machine;

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
