using eBRestarter.Infrastructure.Models.Network;
using eBRestarter.Infrastructure.ObjectArchetypes.Model;

namespace eBRestarter.Infrastructure.Api.Interfaces;

public interface IRestClient
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





