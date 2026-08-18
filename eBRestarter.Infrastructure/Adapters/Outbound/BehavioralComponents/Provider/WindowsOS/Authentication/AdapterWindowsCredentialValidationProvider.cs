using System.DirectoryServices.AccountManagement;
using Microsoft.Extensions.Logging;

using eBRestarter.Core.Application.BehavioralComponents.Extensions;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Authentication;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.ObjectArchetypes.Constants;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Provider.WindowsOS.Authentication;

/// <summary>
/// Adapter: Driven Adapter (Outbound Provider) for validating Windows and Domain credentials.
/// <para>
/// <strong>Architecture Classification: OUTBOUND ADAPTER (Driven Adapter / Provider)</strong><br/>
/// - <strong>Role &amp; Responsibility:</strong> Validates user credentials via Active Directory provider in the Infrastructure layer.<br/>
/// - <strong>Implemented Port:</strong> <see cref="IOutboundPortCredentialValidationProvider"/>.<br/>
/// </para>
/// </summary>
/// <param name="adProviderPort">Active Directory provider port for low-level credential validation.</param>
/// <param name="logger">Logger instance for security events.</param>
public sealed class AdapterWindowsCredentialValidationProvider(
    IOutboundPortActiveDirectoryProvider adProviderPort,
    ILogger<AdapterWindowsCredentialValidationProvider> logger) : IOutboundPortCredentialValidationProvider
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════

    // ── Block 2: Primitives & strings ──
    // 🔒 Security: Events are logged without exposing password credentials.
    private const string CredentialValidationErrorLogMessage = "Credential validation for user {User} in scope {Scope} failed with an unexpected error; treating the credentials as invalid.";
    private const string CredentialValidationRejectedLogMessage = "Credential validation REJECTED for user {User} in scope {Scope}.";
    private const string PrincipalServerDownMessage = "PrincipalServerDown";


    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════

    // ── Block 1: Injected dependencies ──
    private readonly IOutboundPortActiveDirectoryProvider _adProviderPort = adProviderPort ?? throw new ArgumentNullException(nameof(adProviderPort));
    private readonly ILogger<AdapterWindowsCredentialValidationProvider> _logger = logger ?? throw new ArgumentNullException(nameof(logger));


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════

    /// <inheritdoc />
    /// <remarks>
    /// 🔒 Security guideline: Failed credential validation attempts log username and scope with masked identifiers; passwords are never logged.
    /// <para>
    /// The catch-all fallback returning false enforces fail-secure behavior on unexpected exceptions.
    /// </para>
    /// </remarks>
    public bool ValidateCredentials(string username, string domain, string password)
    {
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(password);

        var contextScope = !string.Equals(domain, Environment.MachineName, StringComparison.OrdinalIgnoreCase)
            ? DirectoryContextScope.Domain
            : DirectoryContextScope.Machine;

        try
        {
            bool isValid = _adProviderPort.ValidateCredentials(contextScope, domain, username, password);

            if (!isValid)
            {
                // Mask account identifier in logs.
                _logger.LogWarning(LogEventIds.Security.CredentialValidationRejected, CredentialValidationRejectedLogMessage, LogRedaction.MaskIdentifier(username), contextScope);
            }

            return isValid;
        }
        catch (PrincipalServerDownException)
        {
            throw new InvalidOperationException(PrincipalServerDownMessage);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(LogEventIds.Security.CredentialValidationErrored, exception, CredentialValidationErrorLogMessage, LogRedaction.MaskIdentifier(username), contextScope);

            return false;
        }
    }
}
