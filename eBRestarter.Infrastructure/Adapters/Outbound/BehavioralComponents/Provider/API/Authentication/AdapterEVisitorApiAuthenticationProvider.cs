using System;
using System.Threading.Tasks;

using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Authentication;
using eBRestarter.Infrastructure.Api;
using eBRestarter.Infrastructure.Api.Interfaces;
using eBRestarter.Infrastructure.ObjectArchetypes.Enums;
using eBRestarter.Infrastructure.ObjectArchetypes.Model;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Provider.API.Authentication;

/// <summary>
/// Adapter: Driven Adapter (Outbound Provider) for verifying user credentials against the eBesucher API.
/// <para>
/// <strong>Architecture Classification: OUTBOUND ADAPTER (Driven Adapter / Provider)</strong><br/>
/// - <strong>Role &amp; Responsibility:</strong> Handles external API authentication checks via HTTP REST in the Infrastructure layer.<br/>
/// - <strong>Implemented Port:</strong> <see cref="IOutboundPortApiAuthenticationProvider"/>.<br/>
/// </para>
/// </summary>
public sealed class AdapterEVisitorApiAuthenticationProvider : IOutboundPortApiAuthenticationProvider
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitives & strings ──
    private const string ConnectionSuccessfulMessage = "Connection successful!";
    private const string ErrorPrefix = "Error: ";
    private const string InvalidCredentialsErrorMessage = "Invalid username or API key.";
    private const int RequestTimeoutSeconds = 10;

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injected dependencies ──
    private readonly IRestClient _restClientPort;


    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Initializes a new instance of <see cref="AdapterEVisitorApiAuthenticationProvider"/>.
    /// </summary>
    /// <param name="restClientPort">REST client port for executing API requests.</param>
    public AdapterEVisitorApiAuthenticationProvider(IRestClient restClientPort)
    {
        ArgumentNullException.ThrowIfNull(restClientPort);

        _restClientPort = restClientPort;
    }


    // ═══════════════════════════════════════════════════════
    //  8. Methods (public → private)
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Verifies user credentials against the eBesucher API.
    /// </summary>
    /// <param name="username">eBesucher username.</param>
    /// <param name="apiKey">eBesucher API key.</param>
    public Task<VerificationResult> VerifyCredentialsAsync(string username, string apiKey)
    {
        // Guard clauses for parameter validation
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(apiKey);

        return VerifyCredentialsCoreAsync(username, apiKey);
    }

    /// <summary>
    /// Executes the credential verification request via the REST client.
    /// </summary>
    /// <param name="username">eBesucher username.</param>
    /// <param name="apiKey">eBesucher API key.</param>
    private async Task<VerificationResult> VerifyCredentialsCoreAsync(string username, string apiKey)
    {
        var request = new ApiRequest
        {
            Url = ApiWebLinks.HourlyEarnings,
            Username = username,
            Password = apiKey,
            TimeoutSeconds = RequestTimeoutSeconds
        };

        var response = await _restClientPort.ExecuteGetAsync(request).ConfigureAwait(false);

        return response switch
        {
            { IsSuccess: true } => new VerificationResult(true, ConnectionSuccessfulMessage),
            { StatusCode: ResponseCode.HttpRE401 } => new VerificationResult(false, InvalidCredentialsErrorMessage),
            _ => new VerificationResult(false, $"{ErrorPrefix}{response.ErrorMessage}")
        };
    }
}
