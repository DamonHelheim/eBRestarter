using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.Services.Interfaces
{
    public interface INavigationService
    {
        // =========================================================
        // 1. PUBLIC METHODS (API / Contract)
        // =========================================================
        #region PublicMethods

        void AttachFrame(Frame frame);
        bool NavigateTo(string key, object parameter = null, NavigationTransitionInfo transitionInfo = null);
        void RegisterRoute(string key, Type pageType);

        #endregion
    }
}
