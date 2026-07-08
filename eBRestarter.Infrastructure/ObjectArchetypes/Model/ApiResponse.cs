using eBRestarter.Infrastructure.Enums;

namespace eBRestarter.Infrastructure.ObjectArchetypes.Model;

public sealed class ApiResponse
{
    public bool IsSuccess { get; set; }
    public string? Content { get; set; }
    public ResponseCode StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
}


