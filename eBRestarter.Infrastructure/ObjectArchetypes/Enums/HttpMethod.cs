namespace eBRestarter.Infrastructure.ObjectArchetypes.Enums;

/// <summary>
/// Specifies the HTTP request method used for REST API communication.
/// </summary>
public enum HttpMethod
{
    /// <summary>
    /// HTTP GET method for data retrieval.
    /// </summary>
    GET,

    /// <summary>
    /// HTTP POST method for entity creation or submission.
    /// </summary>
    POST,

    /// <summary>
    /// HTTP PUT method for entity replacement or update.
    /// </summary>
    PUT,

    /// <summary>
    /// HTTP DELETE method for resource deletion.
    /// </summary>
    DELETE,

    /// <summary>
    /// HTTP PATCH method for partial resource modification.
    /// </summary>
    PATCH
}
