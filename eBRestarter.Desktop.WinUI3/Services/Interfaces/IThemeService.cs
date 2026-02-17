using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Desktop.WinUI3.Services.Interfaces
{
    public interface IThemeService
    {
        // =========================================================
        // 1. PUBLIC PROPERTIES (Contract)
        // =========================================================
        #region PublicProperties

        string CurrentTheme { get; }

        #endregion

        // =========================================================
        // 2. PUBLIC METHODS (API / Contract)
        // =========================================================
        #region PublicMethods

        void SetTheme(string themeName);

        #endregion
    }
}
