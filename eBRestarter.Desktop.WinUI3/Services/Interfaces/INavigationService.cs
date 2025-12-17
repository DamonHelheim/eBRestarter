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
        void AttachFrame(Frame frame);

        bool NavigateTo(string key, object parameter = null, NavigationTransitionInfo transitionInfo = null);

        // Registriert eine Route (Mapping: String -> Page Type)
        void RegisterRoute(string key, Type pageType);
    }
}

//public interface INavigationService
//{
//    bool Navigate<TPage>(object? parameter = null, NavigationTransitionInfo? infoOverride = null) where TPage : Page;

//    bool Navigate(Type pageType, object? parameter = null, NavigationTransitionInfo? infoOverride = null);

//    void AttachFrame(Frame frame);

//    bool NavigateTo(string key, object parameter = null, NavigationTransitionInfo transitionInfo = null);

//    // Registriert eine Route (Mapping: String -> Page Type)
//    void RegisterRoute(string key, Type pageType);

//    // Navigation zurück
//    bool GoBack();

//    // Prüfen, ob zurück möglich ist
//    bool CanGoBack { get; }
//}
