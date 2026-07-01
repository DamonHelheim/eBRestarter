using eBRestarter.Core.Application.Ports.Outbound.Authentication;
using System.DirectoryServices.AccountManagement;

namespace eBRestarter.Infrastructure.Adapters.Authentication;

/// <summary>
/// This is the actual wrapper for the Windows API, which we replace with Moq in unit tests.
/// It contains no business logic of its own and merely passes the parameters through to Windows.
/// </summary>
public sealed class WindowsActiveDirectoryProvider : IActiveDirectoryProviderOutboundPort
{
    public bool ValidateCredentials(ContextType contextType, string domain, string username, string password)
    {
        using var context = new PrincipalContext(contextType, domain);

        return context.ValidateCredentials(username, password);
    }
}
