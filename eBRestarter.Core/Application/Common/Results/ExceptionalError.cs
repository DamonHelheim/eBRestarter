namespace eBRestarter.Core.Application.Common.Results;

/// <summary>
/// Represents a specialized error caused by an unhandled exception.
/// </summary>
public class ExceptionalError : Error
{
    /// <summary>
    /// Gets the underlying exception that caused the error.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionalError"/> class wrapping the specified exception.
    /// </summary>
    /// <param name="exception">The exception instance associated with this error.</param>
    public ExceptionalError(Exception exception) : base(exception.Message)
    {
        Exception = exception;
    }
}
