using eBRestarter.Infrastructure.ObjectArchetypes.Model;

namespace eBRestarter.Infrastructure.Api.Interfaces;

/// <summary>
/// Defines operations for executing synchronous and asynchronous HTTP GET requests against external REST APIs.
/// </summary>
public interface IRestClient
{
    /// <summary>
    /// Executes an asynchronous HTTP GET request with the specified configuration.
    /// </summary>
    /// <param name="request">The API request details including target URL, timeout, and authentication credentials.</param>
    /// <returns>The result of the HTTP request encapsulated in an <see cref="ApiResponse"/>.</returns>
    Task<ApiResponse> ExecuteGetAsync(ApiRequest request);

    /// <summary>
    /// Executes a synchronous HTTP GET request with the specified configuration.
    /// </summary>
    /// <param name="request">The API request details including target URL, timeout, and authentication credentials.</param>
    /// <returns>The result of the HTTP request encapsulated in an <see cref="ApiResponse"/>.</returns>
    ApiResponse ExecuteGet(ApiRequest request);
}





