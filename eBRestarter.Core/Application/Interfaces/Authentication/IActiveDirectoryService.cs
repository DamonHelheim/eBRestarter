using System.DirectoryServices.AccountManagement;

namespace eBRestarter.Core.Application.Interfaces.Authentication;

public interface IActiveDirectoryService
{
    bool ValidateCredentials(ContextType contextType, string domain, string username, string password);
}
