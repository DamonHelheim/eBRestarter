using System.DirectoryServices.AccountManagement;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Authentication;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Provider.WindowsOS.Authentication;

/// <summary>
/// Adapter: Driven Adapter (Outbound Provider) wrapping Windows Active Directory / PrincipalContext credential validation.
/// <para>
/// <strong>Architecture Classification: OUTBOUND ADAPTER (Driven Adapter / Provider)</strong><br/>
/// - <strong>Role &amp; Responsibility:</strong> Encapsulates native Windows Active Directory and PrincipalContext APIs in the Infrastructure layer.<br/>
/// - <strong>Implemented Port:</strong> <see cref="IOutboundPortActiveDirectoryProvider"/>.<br/>
/// </para>
/// </summary>
public sealed class AdapterWindowsActiveDirectoryProvider : IOutboundPortActiveDirectoryProvider
{
    /// <summary>
    /// Validates Windows credentials against Active Directory or local machine security context.
    /// </summary>
    /// <param name="contextScope">The target security context scope (Domain, Machine, ApplicationDirectory).</param>
    /// <param name="domain">The target domain name or machine name.</param>
    /// <param name="username">The username to authenticate.</param>
    /// <param name="password">The password to validate.</param>
    public bool ValidateCredentials(DirectoryContextScope contextScope, string domain, string username, string password)
    {
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(password);

        var contextType = contextScope switch
        {
            DirectoryContextScope.Domain => ContextType.Domain,
            DirectoryContextScope.ApplicationDirectory => ContextType.ApplicationDirectory,
            _ => ContextType.Machine
        };

        string? targetDomain = string.IsNullOrWhiteSpace(domain) ? null : domain;

        using var context = new PrincipalContext(contextType, targetDomain);

        return context.ValidateCredentials(username, password);
    }
}
