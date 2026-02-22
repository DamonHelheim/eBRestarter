using eBRestarter.Core.Domain.Models;

namespace eBRestarter.Core.Application.Interfaces.RestClient;

public interface IRestClientService
{
    // Asynchron ist Standard für Web-Requests
    Task<ApiResponse> ExecuteGetAsync(ApiRequest request);

    // Synchron (falls zwingend nötig, aber async preferred)
    ApiResponse ExecuteGet(ApiRequest request);
}
