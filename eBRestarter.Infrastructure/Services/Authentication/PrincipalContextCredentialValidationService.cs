using eBRestarter.Core.Application.Interfaces.Authentication;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using System.DirectoryServices.AccountManagement;
using Windows.Services.Maps;

namespace eBRestarter.Infrastructure.Services.Authentication;

/// <summary>
/// Validierung von Windows-/Domain-Anmeldedaten via PrincipalContext (Move aus ViewModelOptions).
/// </summary>
public class PrincipalContextCredentialValidationService(IActiveDirectoryService adService) : ICredentialValidationService
{
    private readonly IActiveDirectoryService _adService = adService;

    #region PublicAndProtectedMethods

    /// <inheritdoc />
    public bool ValidateCredentials(string username, string domain, string password)
    {
        try
        {
            ContextType contextType = ContextType.Machine;

            if (!string.Equals(domain, Environment.MachineName, StringComparison.OrdinalIgnoreCase))
            {
                contextType = ContextType.Domain;
            }

            // Hier rufen wir jetzt das gemockte Interface auf anstatt 'new' zu benutzen!
            return _adService.ValidateCredentials(contextType, domain, username, password);
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

    #endregion
}
