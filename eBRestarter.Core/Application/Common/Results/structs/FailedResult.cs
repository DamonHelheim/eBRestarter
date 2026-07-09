namespace eBRestarter.Core.Application.Common.Results.structs;

public readonly struct FailedResult
{
    public IReadOnlyList<Error> Errors { get; }

    public FailedResult(IEnumerable<Error> errors)
    {
        Errors = [.. errors];
    }

    public bool HasError<TError>() where TError : Error => Errors.Any(e => e is TError);

    public static implicit operator Result(FailedResult failedResult) => Result.FailFromStruct(failedResult.Errors);
}
