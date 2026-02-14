using eBRestarter.Core.Application.Interfaces;

namespace eBRestarter.Core.Application.Services
{
    /// <summary>
    /// Domain-Regel: Erlaubte Cache-Lösch-Intervalle (Move aus ViewModelRestartTask IsIntervalAllowed).
    /// </summary>
    public class CacheDeletionIntervalValidator : ICacheDeletionIntervalValidator
    {
        // =========================================================
        // 1. PUBLIC & PROTECTED METHODS (API)
        // =========================================================
        #region PublicAndProtectedMethods

        /// <inheritdoc />
        public bool IsValidIntervalDays(int days) => days switch
        {
            1 or 3 or 7 or 14 => true,
            _ => false
        };

        #endregion
    }
}
