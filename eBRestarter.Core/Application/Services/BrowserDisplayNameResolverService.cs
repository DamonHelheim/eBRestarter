using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Domain.Enums;

namespace eBRestarter.Core.Application.Services
{
    /// <summary>
    /// Anzeigename → BrowserType (Move aus ViewModelRestartTask GetBrowserTypeFromString).
    /// </summary>
    public class BrowserDisplayNameResolverService : IBrowserDisplayNameResolver
    {
        // =========================================================
        // 1. PUBLIC & PROTECTED METHODS (API)
        // =========================================================
        #region PublicAndProtectedMethods

        /// <inheritdoc />
        public BrowserType GetBrowserTypeFromDisplayName(string displayName, string defaultDisplayText)
        {
            if (string.IsNullOrWhiteSpace(displayName) || displayName == defaultDisplayText || displayName == "Nicht gewählt")
                return BrowserType.Chrome;

            if (Enum.TryParse(displayName, true, out BrowserType type))
                return type;

            return BrowserType.Chrome;
        }

        #endregion
    }
}
