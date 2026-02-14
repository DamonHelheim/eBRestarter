namespace eBRestarter.Core.Application.Interfaces
{
    /// <summary>
    /// Port für die Prüfung, ob ein Cache-Lösch-Intervall (Tage) erlaubt ist.
    /// </summary>
    public interface ICacheDeletionIntervalValidator
    {
        // =========================================================
        // 1. PUBLIC METHODS (API / Vertrag)
        // =========================================================
        #region PublicMethods

        /// <summary>
        /// Gibt true zurück für erlaubte Intervalle (1, 3, 7, 14 Tage), sonst false.
        /// </summary>
        bool IsValidIntervalDays(int days);

        #endregion
    }
}
