namespace eBRestarter.Core.Domain.Validators;

/// <summary>
/// Validation to check if a cache deletion interval (in days) is permitted.
/// </summary>
public interface ICacheDeletionIntervalValidator
{
    /// <summary>
    /// Returns true for permitted intervals (1, 3, 7, 14 days), otherwise false.
    /// </summary>
    bool IsValidIntervalDays(int days);
}