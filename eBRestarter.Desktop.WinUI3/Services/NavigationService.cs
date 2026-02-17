using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.Services
{
    /// <summary>
    /// Central navigation service: registers routes and navigates via an abstracted frame adapter.
    /// </summary>
    public sealed class NavigationService : INavigationService
    {
        // =========================================================
        // 1. FIELDS & INJECTED SERVICES (Backing state)
        // =========================================================
        #region FieldsAndInjectedServices

        private readonly Dictionary<string, Type> _pages = [];
        private INavigationFrame? _frameAdapter;

        #endregion

        // =========================================================
        // 2. PUBLIC METHODS
        // =========================================================
        #region PublicMethods

        public void AttachFrame(Frame frame)
        {
            _frameAdapter = new WinUIFrameAdapter(frame);
        }

        internal void SetFrameAdapter(INavigationFrame frameAdapter)
        {
            _frameAdapter = frameAdapter;
        }

        public bool NavigateTo(string key, object parameter = null, NavigationTransitionInfo transitionInfo = null)
        {
            if (_frameAdapter == null) return false;
            if (_pages.TryGetValue(key, out var pageType))
            {
                if (_frameAdapter.Content?.GetType() == pageType) return false;
                return _frameAdapter.Navigate(pageType, parameter, transitionInfo);
            }
            return false;
        }

        public void RegisterRoute(string key, Type pageType)
        {
            if (!_pages.ContainsKey(key))
                _pages.Add(key, pageType);
        }

        #endregion
    }
}
