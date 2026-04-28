namespace eBRestarter.Core.Application.Models.Api;

public class ApiRequest
{
    public string Url { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public global::eBRestarter.Core.Application.Enums.HttpMethod Method { get; set; }
    public int TimeoutSeconds { get; set; } = 60;

}
