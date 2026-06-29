using FluentResults;

namespace eBRestarter.Core.Application.Models.Errors;

public sealed class ProcessConflictError : Error
{
    public ProcessConflictError(string message) : base(message)
    {
    }
}

