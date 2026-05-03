using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;

namespace eBRestarter.Desktop.WinUI3.Services.Interfaces;

public interface INavigationService
{
    void AttachFrame(Frame frame);
    bool NavigateTo(string key, object parameter = null!, NavigationTransitionInfo transitionInfo = null!);
    void RegisterRoute(string key, Type pageType);
}
