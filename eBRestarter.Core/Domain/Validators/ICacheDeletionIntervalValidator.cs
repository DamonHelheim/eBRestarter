namespace eBRestarter.Core.Domain.Validators;

/// <summary>
/// Validation to check if a cache deletion interval (in days) is permitted.
/// </summary>
public interface ICacheDeletionIntervalValidator
{
    /// <summary>
    /// Checks whether the specified cache deletion interval in days is permitted.
    /// </summary>
    /// <param name="days">The candidate cleanup interval in days.</param>
    /// <returns><c>true</c> if the interval is permitted (1, 3, 7, or 14 days); otherwise, <c>false</c>.</returns>
    bool IsValidIntervalDays(int days);
}