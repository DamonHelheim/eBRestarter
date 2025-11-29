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
        bool Navigate<TPage>(
            object? parameter = null,
            NavigationTransitionInfo? infoOverride = null
        ) where TPage : Page;

        bool Navigate(
            Type pageType,
            object? parameter = null,
            NavigationTransitionInfo? infoOverride = null
        );

        void AttachFrame(Frame frame);
    }
}
