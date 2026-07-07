using eBRestarter.Core.Application.Common.Results;

namespace eBRestarter.Core.Application.Models;

/// <summary>
/// Represents a strongly typed error indicating that an operation failed because a target process
/// (e.g., a web browser) is currently running and locking resources.
/// <para>
/// <b>Architectural Note (Typed Error / Marker Type):</b><br/>
/// Although this class does not define additional members or execute custom logic in its body,
/// its distinct type identity serves a critical purpose in error handling. By deriving from <see cref="Error"/>,
/// it acts as a semantic marker that allows calling layers (such as UI ViewModels) to perform type-safe
/// pattern matching or check <c>result.HasError&lt;ProcessConflictError&gt;()</c>. This avoids fragile,
/// error-prone string parsing of error messages and enables reliable, localized error-routing workflows
/// (e.g., prompting the user to force-close the running browser).
/// </para>
/// </summary>
public sealed class ProcessConflictError : Error
{
    public ProcessConflictError(string message) : base(message)
    {
    }
}

