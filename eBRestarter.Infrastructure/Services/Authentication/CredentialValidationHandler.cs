using eBRestarter.Core.Application.Interfaces.Authentication;
using System.DirectoryServices.AccountManagement;

namespace eBRestarter.Infrastructure.Services.Authentication;

/// <summary>
/// Validierung von Windows-/Domain-Anmeldedaten via PrincipalContext (Move aus ViewModelOptions).
/// </summary>
public class CredentialValidationHandler(IActiveDirectoryUseCase adUseCase) : ICredentialValidationUseCase
{
    private readonly IActiveDirectoryUseCase _adUseCase = adUseCase;

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

            // Hier rufen wir jetzt das gemockte Interface auf anstatt 'new' zu benutzen!
            return _adUseCase.ValidateCredentials(contextType, domain, username, password);
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

