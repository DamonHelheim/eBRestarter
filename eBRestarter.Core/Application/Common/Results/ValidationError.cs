namespace eBRestarter.Core.Application.Common.Results;

/// <summary>
/// Represents a validation failure message resulting from request or domain validation rules.
/// </summary>
/// <param name="ErrorMessage">The detailed validation failure message.</param>
public sealed record ValidationError(string ErrorMessage);
