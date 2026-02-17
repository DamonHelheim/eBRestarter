using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Desktop.WinUI3.Services
{
    /// <summary>
    /// Adapter that implements <see cref="INavigationFrame"/> and delegates to a WinUI <see cref="Frame"/>.
    /// </summary>
    public class WinUIFrameAdapter : INavigationFrame
    {
        // =========================================================
        // 1. FIELDS & INJECTED SERVICES (Backing state)
        // =========================================================
        #region FieldsAndInjectedServices

        private readonly Frame _frame;

        #endregion

        // =========================================================
        // 2. CONSTRUCTOR & FINALIZER (Ctor)
        // =========================================================
        #region ConstructorAndFinalizer

        public WinUIFrameAdapter(Frame frame) => _frame = frame;

        #endregion

        // =========================================================
        // 3. PUBLIC PROPERTIES (Data & State)
        // =========================================================
        #region PublicProperties

        public bool CanGoBack => _frame.CanGoBack;
        public object Content => _frame.Content;

        #endregion

        // =========================================================
        // 4. PUBLIC METHODS (API)
        // =========================================================
        #region PublicMethods

        public void GoBack() => _frame.GoBack();

        public bool Navigate(Type sourcePageType, object parameter, NavigationTransitionInfo infoOverride)
        {
            return _frame.Navigate(sourcePageType, parameter, infoOverride);
        }

        #endregion
    }
}
