namespace eBRestarter.Core.Application.Common.Results.structs;

/// <summary>
/// Represents an intermediate failed result struct carrying a list of errors for implicit conversion to <see cref="Result"/> or <see cref="Result{TValue}"/>.
/// </summary>
public readonly struct FailedResult
{
    /// <summary>
    /// Gets the read-only list of errors associated with the failed result.
    /// </summary>
    public IReadOnlyList<Error> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FailedResult"/> struct with the specified errors.
    /// </summary>
    /// <param name="errors">The collection of errors causing the failure.</param>
    public FailedResult(IEnumerable<Error> errors)
    {
        Errors = [.. errors];
    }

    /// <summary>
    /// Checks whether the failure contains an error of the specified type <typeparamref name="TError"/>.
    /// </summary>
    /// <typeparam name="TError">The type of error to check for.</typeparam>
    /// <returns><see langword="true"/> if an error of type <typeparamref name="TError"/> exists; otherwise, <see langword="false"/>.</returns>
    public bool HasError<TError>() where TError : Error => Errors.Any(e => e is TError);

    /// <summary>
    /// Implicitly converts a <see cref="FailedResult"/> struct to a <see cref="Result"/> instance.
    /// </summary>
    /// <param name="failedResult">The failed result struct instance.</param>
    public static implicit operator Result(FailedResult failedResult) => Result.FailFromStruct(failedResult.Errors);
}
