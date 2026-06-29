using eBRestarter.Infrastructure.Api;

namespace eBRestarter.Infrastructure.Network;

public sealed class ApiResponse
{
    public bool IsSuccess { get; set; }
    public string? Content { get; set; } // Das JSON
    public ResponseCode StatusCode { get; set; } // Dein Enum
    public string? ErrorMessage { get; set; }
}


