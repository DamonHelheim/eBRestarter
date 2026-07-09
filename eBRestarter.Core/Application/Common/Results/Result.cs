using eBRestarter.Core.Application.Common.Results.structs;

namespace eBRestarter.Core.Application.Common.Results;

public class Result
{
    public bool IsSuccess => Errors.Count == 0;

    public bool IsFailed => Errors.Count > 0;

    public IReadOnlyList<Error> Errors { get; }

    protected Result(IEnumerable<Error> errors)
    {
        Errors = [.. errors];
    }

    public static Result Ok() => new([]);

    public static Result<TValue> Ok<TValue>(TValue value) => new(value, []);

    public static FailedResult Fail(string errorMessage) => new([new Error(errorMessage)]);

    public static FailedResult Fail(Error error) => new([error]);

    public static Result<TValue> Fail<TValue>(Error error) => new(default!, [error]);

    public static Result<TValue> Fail<TValue>(string errorMessage) => new(default!, [new Error(errorMessage)]);

    internal static Result FailFromStruct(IEnumerable<Error> errors) => new(errors);

    public bool HasError<TError>() where TError : Error
    {
        return Errors.Any(e => e is TError);
    }
}
