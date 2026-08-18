using eBRestarter.Infrastructure.ObjectArchetypes.Enums;

namespace eBRestarter.Infrastructure.ObjectArchetypes.Model;

/// <summary>
/// Model representing the outcome and payload of an HTTP REST API response.
/// </summary>
public sealed class ApiResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the request succeeded with a 2xx status code.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets the raw string content of the response body.
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Gets or sets the status code of the response.
    /// </summary>
    public ResponseCode StatusCode { get; set; }

    /// <summary>
    /// Gets or sets error details if the request failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
