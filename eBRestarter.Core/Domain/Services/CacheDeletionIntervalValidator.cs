namespace eBRestarter.Core.Domain.Services;

/// <summary>
/// Fachregel: Erlaubte Cache-Lösch-Intervalle.
/// </summary>
public class CacheDeletionIntervalValidator : ICacheDeletionIntervalValidator
{
    /// <inheritdoc />
    public bool IsValidIntervalDays(int days) => days switch
    {
        1 or 3 or 7 or 14 => true,
        _ => false
    };
}
