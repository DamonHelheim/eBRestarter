using System.DirectoryServices.AccountManagement;

namespace eBRestarter.Core.Application.Ports.Outbound.Authentication;

public interface IActiveDirectoryPort
{
    /// <summary>
    /// Validates the user credentials against Active Directory.
    /// This is a read-only query that checks an external state and returns a yes/no result
    /// without modifying any data within our own system, making it a clean Provider.
    /// </summary>
    bool ValidateCredentials(ContextType contextType, string domain, string username, string password);
}

