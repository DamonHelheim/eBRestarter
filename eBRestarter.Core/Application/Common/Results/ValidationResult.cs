namespace eBRestarter.Core.Application.Common.Results;

/// <summary>
/// Represents the aggregated result of a validation check.
/// </summary>
/// <param name="IsValid">Indicates whether all validation rules passed.</param>
/// <param name="Errors">The read-only list of validation errors, if any.</param>
public sealed record ValidationResult(bool IsValid, IReadOnlyList<ValidationError> Errors)
{
    /// <summary>
    /// Creates a successful <see cref="ValidationResult"/> instance with no validation errors.
    /// </summary>
    /// <returns>A successful <see cref="ValidationResult"/> instance.</returns>
    public static ValidationResult Success() => new(true, []);

    /// <summary>
    /// Creates a failed <see cref="ValidationResult"/> instance with a single error message.
    /// </summary>
    /// <param name="errorMessage">The error message explaining the validation failure.</param>
    /// <returns>A failed <see cref="ValidationResult"/> instance.</returns>
    public static ValidationResult Fail(string errorMessage) => new(false, [new ValidationError(errorMessage)]);

    /// <summary>
    /// Creates a failed <see cref="ValidationResult"/> instance with multiple error messages.
    /// </summary>
    /// <param name="errorMessages">The collection of error messages causing the validation failure.</param>
    /// <returns>A failed <see cref="ValidationResult"/> instance.</returns>
    public static ValidationResult Fail(IEnumerable<string> errorMessages) => new(false, [.. errorMessages.Select(m => new ValidationError(m))]);
}
