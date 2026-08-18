namespace eBRestarter.Infrastructure.ObjectArchetypes.Model;

/// <summary>
/// Model representing an outgoing HTTP REST API request.
/// </summary>
public sealed class ApiRequest
{
    /// <summary>
    /// Gets or sets the target URL endpoint for the request.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional username for basic authentication.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the optional password or API key for authentication.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets the HTTP method used for the request.
    /// </summary>
    public Enums.HttpMethod Method { get; set; }

    /// <summary>
    /// Gets or sets the request timeout in seconds (default is 60).
    /// </summary>
    public int TimeoutSeconds { get; set; } = 60;
}
