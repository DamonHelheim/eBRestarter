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
    public sealed class NavigationService : INavigationService
    {
        private Frame? _frame;

        public void AttachFrame(Frame frame) => _frame = frame;

        public bool Navigate<TPage>(
            object? parameter = null,
            NavigationTransitionInfo? infoOverride = null
        ) where TPage : Page
        {
            if (_frame is null) return false;

            if (infoOverride is null)
            {
                // Standard-Navigation ohne explizite Transition
                return _frame.Navigate(typeof(TPage), parameter);
            }

            return _frame.Navigate(typeof(TPage), parameter, infoOverride);
        }

        public bool Navigate(
            Type pageType,
            object? parameter = null,
            NavigationTransitionInfo? infoOverride = null
        )
        {
            if (_frame is null) return false;

            if (infoOverride is null)
            {
                return _frame.Navigate(pageType, parameter);
            }

            return _frame.Navigate(pageType, parameter, infoOverride);
        }
    }
}
