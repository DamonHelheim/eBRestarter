namespace eBRestarter.Core.Application.Common.Results;

public class ExceptionalError : Error
{
    public Exception Exception { get; }

    public ExceptionalError(Exception exception) : base(exception.Message)
    {
        Exception = exception;
    }
}
