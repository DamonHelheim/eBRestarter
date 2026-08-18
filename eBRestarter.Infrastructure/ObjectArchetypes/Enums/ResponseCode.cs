namespace eBRestarter.Infrastructure.ObjectArchetypes.Enums;

/// <summary>
/// Represents response status and outcome codes for API and network operations.
/// </summary>
public enum ResponseCode
{
    /// <summary>
    /// General unhandled exception during request execution.
    /// </summary>
    GeneralExceptionError = -3,

    /// <summary>
    /// General operation error.
    /// </summary>
    Error = -1,

    /// <summary>
    /// No status code set.
    /// </summary>
    None = 0,

    /// <summary>
    /// Operation completed successfully.
    /// </summary>
    Success = 1,

    /// <summary>
    /// Client-side rate limit reached.
    /// </summary>
    RequestLimit = 10,

    /// <summary>
    /// HTTP 401 Unauthorized response.
    /// </summary>
    HttpRE401 = 401,

    /// <summary>
    /// Server unreachable or route not found (HTTP 404).
    /// </summary>
    NoConnectionToServer = 404,

    /// <summary>
    /// Request timed out (HTTP 408).
    /// </summary>
    HTTPTimeout = 408,

    /// <summary>
    /// HTTP 429 Too Many Requests response.
    /// </summary>
    HttpRE429 = 429,

    /// <summary>
    /// HTTP 500 Internal Server Error response.
    /// </summary>
    InternalServerError = 500,

    /// <summary>
    /// HTTP 501 / 500 server error variant.
    /// </summary>
    HttpRE500 = 501,

    /// <summary>
    /// Bad gateway or upstream error (HTTP 502).
    /// </summary>
    HttpRE200 = 502
}
