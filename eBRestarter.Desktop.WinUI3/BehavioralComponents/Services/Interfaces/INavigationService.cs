using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;

namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Services.Interfaces;

/// <summary>
/// Service interface for registering route mapping keys and controlling application page navigation.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Attaches the native WinUI 3 frame container to the navigation service.
    /// </summary>
    /// <param name="frame">The native WinUI 3 <see cref="Frame"/> instance.</param>
    void AttachFrame(Frame frame);

    /// <summary>
    /// Navigates to the page registered under the specified route key.
    /// </summary>
    /// <param name="key">The unique route key identifier.</param>
    /// <param name="parameter">Optional navigation parameter passed to the target page.</param>
    /// <param name="transitionInfo">Optional transition animation information.</param>
    /// <returns><c>true</c> if navigation succeeded; otherwise, <c>false</c>.</returns>
    bool NavigateTo(string key, object parameter = null!, NavigationTransitionInfo transitionInfo = null!);

    /// <summary>
    /// Registers a route key and associates it with a page target type.
    /// </summary>
    /// <param name="key">The unique route key identifier.</param>
    /// <param name="pageType">The target page type.</param>
    void RegisterRoute(string key, Type pageType);
}
