namespace eBRestarter.Core.Domain.Services;

/// <summary>
/// Prüfung, ob ein Cache-Lösch-Intervall (Tage) erlaubt ist.
/// </summary>
public interface ICacheDeletionIntervalValidator
{
    /// <summary>
    /// Gibt true zurück für erlaubte Intervalle (1, 3, 7, 14 Tage), sonst false.
    /// </summary>
    bool IsValidIntervalDays(int days);
}
