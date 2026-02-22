using eBRestarter.Core.Application.Interfaces;

namespace eBRestarter.Core.Application.Services;

/// <summary>
/// Reine Berechnungslogik für den nächsten Neustart-Termin (Move aus ViewModelOptions).
/// </summary>
public class RestartCalculationService : IRestartCalculationService
{
    // =========================================================
    // 1. PUBLIC & PROTECTED METHODS (API)
    // =========================================================
    #region PublicAndProtectedMethods

    /// <inheritdoc />
    public DateTime GetNextRestartDate(int intervalDays, int restartClockTime)
    {
        if (intervalDays > 0)
        {
            return DateTime.Today.AddDays(intervalDays).AddHours(restartClockTime);
        }

        return DateTime.MinValue;
    }

    #endregion
}
