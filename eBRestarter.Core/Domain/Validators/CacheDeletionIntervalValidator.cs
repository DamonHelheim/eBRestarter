namespace eBRestarter.Core.Domain.Validators;

/// <summary>
/// Domain Rule: Permitted cache deletion intervals.
/// </summary>
public sealed class CacheDeletionIntervalValidator : ICacheDeletionIntervalValidator
{
    /// <inheritdoc />
    public bool IsValidIntervalDays(int days)
    {
        return days switch
        {
            1 or 3 or 7 or 14 => true,
            _ => false
        };
    }
}