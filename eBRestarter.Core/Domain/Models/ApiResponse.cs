using eBRestarter.Core.Domain.Enums;

namespace eBRestarter.Core.Domain.Models;

public class ApiResponse
{
    public bool IsSuccess { get; set; }
    public string? Content { get; set; } // Das JSON
    public ResponseCode StatusCode { get; set; } // Dein Enum
    public string? ErrorMessage { get; set; }
}
