namespace eBRestarter.Core.Application.Common.Results;

public class Error
{
    public string Message { get; }

    public Dictionary<string, object> Metadata { get; } = [];

    public Error(string message)
    {
        Message = message;
    }

    public Error WithMetadata(string key, object value)
    {
        Metadata[key] = value;
        return this;
    }
}
