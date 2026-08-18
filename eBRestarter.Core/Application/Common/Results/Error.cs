namespace eBRestarter.Core.Application.Common.Results;

/// <summary>
/// Represents a domain or application error with a descriptive message and key-value metadata.
/// </summary>
public class Error
{
    /// <summary>
    /// Gets the descriptive error message explaining the failure cause.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets additional key-value metadata associated with the error.
    /// </summary>
    public Dictionary<string, object> Metadata { get; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="Error"/> class with the specified error message.
    /// </summary>
    /// <param name="message">The descriptive error message.</param>
    public Error(string message)
    {
        Message = message;
    }

    /// <summary>
    /// Adds or updates a key-value pair in the error metadata.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value associated with the key.</param>
    /// <returns>The current <see cref="Error"/> instance for method chaining.</returns>
    public Error WithMetadata(string key, object value)
    {
        Metadata[key] = value;
        return this;
    }
}
