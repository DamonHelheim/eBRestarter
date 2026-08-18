using eBRestarter.Core.Application.Common.Results.structs;

namespace eBRestarter.Core.Application.Common.Results;

/// <summary>
/// Represents the outcome of an operation returning a payload value of type <typeparamref name="TValue"/>.
/// </summary>
/// <typeparam name="TValue">The type of the payload value returned on success.</typeparam>
public class Result<TValue> : Result
{
    /// <summary>
    /// Gets the payload value returned by a successful operation.
    /// </summary>
    public TValue Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{TValue}"/> class with value and errors.
    /// </summary>
    /// <param name="value">The payload value.</param>
    /// <param name="errors">The collection of errors associated with the result.</param>
    internal Result(TValue value, IEnumerable<Error> errors) : base(errors)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a failed <see cref="Result{TValue}"/> instance from an error collection.
    /// </summary>
    /// <param name="errors">The collection of errors.</param>
    /// <returns>A failed <see cref="Result{TValue}"/> instance.</returns>
    internal static new Result<TValue> FailFromStruct(IEnumerable<Error> errors) => new(default!, errors);

    /// <summary>
    /// Implicitly converts a <see cref="FailedResult"/> struct to a <see cref="Result{TValue}"/> instance.
    /// </summary>
    /// <param name="failedResult">The failed result struct instance.</param>
    public static implicit operator Result<TValue>(FailedResult failedResult) => new(default!, failedResult.Errors);
}
