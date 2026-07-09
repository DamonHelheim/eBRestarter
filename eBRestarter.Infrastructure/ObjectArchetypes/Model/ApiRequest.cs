namespace eBRestarter.Infrastructure.ObjectArchetypes.Model;

public sealed class ApiRequest
{
    public string Url { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public Enums.HttpMethod Method { get; set; }
    public int TimeoutSeconds { get; set; } = 60;

}




