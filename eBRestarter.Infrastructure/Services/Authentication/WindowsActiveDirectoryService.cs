using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using System.DirectoryServices.AccountManagement;

namespace eBRestarter.Infrastructure.Services.Authentication;

/// <summary>
/// Das ist der echte Wrapper für die Windows-API, den wir in den Unit-Tests durch Moq ersetzen.
/// Er enthält keine eigene Logik, sondern reicht die Parameter nur an Windows durch.
/// </summary>
public class WindowsActiveDirectoryService : IActiveDirectoryService
{
    public bool ValidateCredentials(ContextType contextType, string domain, string username, string password)
    {
        using var context = new PrincipalContext(contextType, domain);
        return context.ValidateCredentials(username, password);
    }
}