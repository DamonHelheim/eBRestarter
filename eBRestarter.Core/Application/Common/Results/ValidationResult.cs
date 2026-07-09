namespace eBRestarter.Core.Application.Common.Results
{
    public sealed record ValidationResult(bool IsValid, IReadOnlyList<ValidationError> Errors)
    {
        public static ValidationResult Success() => new(true, []);

        public static ValidationResult Fail(string errorMessage) => new(false, [new ValidationError(errorMessage)]);

        public static ValidationResult Fail(IEnumerable<string> errorMessages) => new(false, [.. errorMessages.Select(m => new ValidationError(m))]);
    }
}
