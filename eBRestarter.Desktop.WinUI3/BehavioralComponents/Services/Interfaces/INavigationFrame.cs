using Microsoft.UI.Xaml.Media.Animation;
using System;

namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Services.Interfaces;

/// <summary>
/// Defines an abstraction for the application's navigation frame (enables testing with mocks).
/// </summary>
public interface INavigationFrame
{
    /// <summary>
    /// Gets a value indicating whether there is at least one entry in back navigation history.
    /// </summary>
    bool CanGoBack { get; }

    /// <summary>
    /// Gets the content object currently displayed in the navigation frame.
    /// </summary>
    object Content { get; }

    /// <summary>
    /// Navigates to the most recent item in back navigation history.
    /// </summary>
    void GoBack();

    /// <summary>
    /// Navigates to the target page type with specified parameters and transition animation.
    /// </summary>
    /// <param name="sourcePageType">The target page type to instantiate and display.</param>
    /// <param name="parameter">The navigation parameter passed to the target page.</param>
    /// <param name="infoOverride">The transition animation override information.</param>
    /// <returns><c>true</c> if navigation succeeded; otherwise, <c>false</c>.</returns>
    bool Navigate(Type sourcePageType, object parameter, NavigationTransitionInfo infoOverride);
}
