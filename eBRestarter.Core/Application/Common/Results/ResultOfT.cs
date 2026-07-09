using eBRestarter.Core.Application.Common.Results.structs;

namespace eBRestarter.Core.Application.Common.Results;

public class Result<TValue> : Result
{
    public TValue Value { get; }

    internal Result(TValue value, IEnumerable<Error> errors) : base(errors)
    {
        Value = value;
    }

    internal static new Result<TValue> FailFromStruct(IEnumerable<Error> errors) => new(default!, errors);

    public static implicit operator Result<TValue>(FailedResult failedResult) => new(default!, failedResult.Errors);
}
