using eBRestarter.Core.Application.Common.Results.structs;

namespace eBRestarter.Core.Application.Common.Results;

/// <summary>
/// Represents the outcome of an operation without a return value, indicating success or failure with associated errors.
/// </summary>
public class Result
{
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess => Errors.Count == 0;

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailed => Errors.Count > 0;

    /// <summary>
    /// Gets the read-only list of errors associated with a failed result.
    /// </summary>
    public IReadOnlyList<Error> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class with errors.
    /// </summary>
    /// <param name="errors">The collection of errors for this result instance.</param>
    protected Result(IEnumerable<Error> errors)
    {
        Errors = [.. errors];
    }

    /// <summary>
    /// Creates a successful <see cref="Result"/> instance with no errors.
    /// </summary>
    /// <returns>A successful <see cref="Result"/> instance.</returns>
    public static Result Ok() => new([]);

    /// <summary>
    /// Creates a successful <see cref="Result{TValue}"/> instance carrying a payload value.
    /// </summary>
    /// <typeparam name="TValue">The type of the payload value.</typeparam>
    /// <param name="value">The payload value returned by the successful operation.</param>
    /// <returns>A successful <see cref="Result{TValue}"/> instance.</returns>
    public static Result<TValue> Ok<TValue>(TValue value) => new(value, []);

    /// <summary>
    /// Creates a <see cref="FailedResult"/> struct with a single error message.
    /// </summary>
    /// <param name="errorMessage">The failure error message.</param>
    /// <returns>A <see cref="FailedResult"/> struct instance.</returns>
    public static FailedResult Fail(string errorMessage) => new([new Error(errorMessage)]);

    /// <summary>
    /// Creates a <see cref="FailedResult"/> struct with the specified error.
    /// </summary>
    /// <param name="error">The error causing the failure.</param>
    /// <returns>A <see cref="FailedResult"/> struct instance.</returns>
    public static FailedResult Fail(Error error) => new([error]);

    /// <summary>
    /// Creates a failed <see cref="Result{TValue}"/> carrying type <typeparamref name="TValue"/> with the specified error.
    /// </summary>
    /// <typeparam name="TValue">The expected payload value type.</typeparam>
    /// <param name="error">The error causing the failure.</param>
    /// <returns>A failed <see cref="Result{TValue}"/> instance.</returns>
    public static Result<TValue> Fail<TValue>(Error error) => new(default!, [error]);

    /// <summary>
    /// Creates a failed <see cref="Result{TValue}"/> carrying type <typeparamref name="TValue"/> with a single error message.
    /// </summary>
    /// <typeparam name="TValue">The expected payload value type.</typeparam>
    /// <param name="errorMessage">The failure error message.</param>
    /// <returns>A failed <see cref="Result{TValue}"/> instance.</returns>
    public static Result<TValue> Fail<TValue>(string errorMessage) => new(default!, [new Error(errorMessage)]);

    /// <summary>
    /// Creates a failed <see cref="Result"/> instance from an error collection.
    /// </summary>
    /// <param name="errors">The collection of errors.</param>
    /// <returns>A failed <see cref="Result"/> instance.</returns>
    internal static Result FailFromStruct(IEnumerable<Error> errors) => new(errors);

    /// <summary>
    /// Checks whether the result contains an error of the specified type <typeparamref name="TError"/>.
    /// </summary>
    /// <typeparam name="TError">The type of error to check for.</typeparam>
    /// <returns><see langword="true"/> if an error of type <typeparamref name="TError"/> exists; otherwise, <see langword="false"/>.</returns>
    public bool HasError<TError>() where TError : Error
    {
        return Errors.Any(e => e is TError);
    }
}
