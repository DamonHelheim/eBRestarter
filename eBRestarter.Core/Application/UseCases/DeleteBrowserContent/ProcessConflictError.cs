using FluentResults;

namespace eBRestarter.Core.Application.UseCases.DeleteBrowserContent;

public class ProcessConflictError : Error
{
    public ProcessConflictError(string message) : base(message)
    {
    }
}
