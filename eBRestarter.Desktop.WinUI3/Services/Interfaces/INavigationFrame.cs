using Microsoft.UI.Xaml.Media.Animation;
using System;

namespace eBRestarter.Desktop.WinUI3.Services.Interfaces;

/// <summary>
/// Defines an abstraction for the application's navigation frame (enables testing with mocks).
/// </summary>
public interface INavigationFrame
{
    bool CanGoBack { get; }
    object Content { get; }

    void GoBack();
    bool Navigate(Type sourcePageType, object parameter, NavigationTransitionInfo infoOverride);

}
