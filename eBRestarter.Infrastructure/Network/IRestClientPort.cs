using eBRestarter.Infrastructure.Network;

namespace eBRestarter.Infrastructure.Network;

public interface IRestClientPort
{
    /// <summary>
    /// Asynchronous is the standard for web requests.
    /// </summary>
    Task<ApiResponse> ExecuteGetAsync(ApiRequest request);

    /// <summary>
    /// Synchronous (if strictly necessary, though async is preferred).
    /// </summary>
    ApiResponse ExecuteGet(ApiRequest request);
}





