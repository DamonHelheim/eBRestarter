using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Desktop.WinUI3.Services.Interfaces
{
    /// <summary>
    /// Defines an abstraction for the application's navigation frame (enables testing with mocks).
    /// </summary>
    public interface INavigationFrame
    {
        // =========================================================
        // 1. PUBLIC PROPERTIES (Contract)
        // =========================================================
        #region PublicProperties

        bool CanGoBack { get; }
        object Content { get; }

        #endregion

        // =========================================================
        // 2. PUBLIC METHODS (API / Contract)
        // =========================================================
        #region PublicMethods

        void GoBack();
        bool Navigate(Type sourcePageType, object parameter, NavigationTransitionInfo infoOverride);

        #endregion
    }
}
