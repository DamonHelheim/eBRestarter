using eBRestarter.Core.Application.Ports.Outbound.Authentication;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using System.DirectoryServices.AccountManagement;

namespace eBRestarter.Infrastructure.Adapters.Authentication;

/// <summary>
/// Validation of Windows/Domain credentials via PrincipalContext (moved from ViewModelOptions).
/// </summary>
public sealed class WindowsCredentialValidationProvider(IActiveDirectoryProviderOutboundPort adProviderPort) : ICredentialValidationProviderOutboundPort
{
    private readonly IActiveDirectoryProviderOutboundPort _adProviderPort = adProviderPort;

    /// <inheritdoc />
    public bool ValidateCredentials(string username, string domain, string password)
    {
        try
        {
            var contextType = ContextType.Machine;

            if (!string.Equals(domain, Environment.MachineName, StringComparison.OrdinalIgnoreCase))
            {
                contextType = ContextType.Domain;
            }

            // We are now calling the mocked interface here instead of using 'new'!
            return _adProviderPort.ValidateCredentials(contextType, domain, username, password);
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



