using eBRestarter.Core.Application.Interfaces.Authentication;
using System.DirectoryServices.AccountManagement;

namespace eBRestarter.Infrastructure.Services.Authentication;

/// <summary>
/// Validierung von Windows-/Domain-Anmeldedaten via PrincipalContext (Move aus ViewModelOptions).
/// </summary>
public class PrincipalContextCredentialValidationService : ICredentialValidationService
{
    // =========================================================
    // 1. PUBLIC & PROTECTED METHODS (API)
    // =========================================================
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

            using (var context = new PrincipalContext(contextType, domain))
            {
                return context.ValidateCredentials(username, password);
            }
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
