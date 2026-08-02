using System;
using System.Collections.Generic;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

using eBRestarter.Desktop.WinUI3.BehavioralComponents.Services.Interfaces;
using eBRestarter.Desktop.WinUI3.BehavioralUIComponents.Navigation;

namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Services;

/// <summary>
/// Central navigation service: registers routes and navigates via an abstracted frame adapter.
/// </summary>
public sealed class NavigationService : INavigationService
{
    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injizierte Abhängigkeiten / Schnittstellen ──
    private INavigationFrame? _frameAdapter;

    // ── Block 4: Komplexe Typen / Dictionaries (alphabetisch A–Z) ──
    private readonly Dictionary<string, Type> _pages = [];


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Attaches the native WinUI3 frame container to the navigation service.
    /// </summary>
    /// <param name="frame">The native WinUI3 <see cref="Frame"/> instance.</param>
    public void AttachFrame(Frame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        _frameAdapter = new WinUIFrameNavigator(frame);
    }

    /// <summary>
    /// Navigates to the page registered under the specified route key.
    /// </summary>
    /// <param name="key">The unique route key identifier.</param>
    /// <param name="parameter">Optional navigation parameter passed to the target page.</param>
    /// <param name="transitionInfo">Optional transition animation information.</param>
    /// <returns><c>true</c> if navigation succeeded; <c>false</c> if route is unregistered, frame is unattached, or page is already active.</returns>
    public bool NavigateTo(
        string key,
        object parameter = null!,
        NavigationTransitionInfo transitionInfo = null!)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (_frameAdapter is null)
        {
            return false;
        }

        if (!_pages.TryGetValue(key, out var pageType))
        {
            return false;
        }

        if (_frameAdapter.Content?.GetType() == pageType)
        {
            return false;
        }

        return _frameAdapter.Navigate(pageType, parameter!, transitionInfo!);
    }

    /// <summary>
    /// Registers a route key and associates it with a page target type.
    /// </summary>
    /// <param name="key">The unique route key identifier.</param>
    /// <param name="pageType">The target page type.</param>
    public void RegisterRoute(string key, Type pageType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(pageType);

        _pages.TryAdd(key, pageType);
    }

    /// <summary>
    /// Directly sets an internal frame adapter (primarily for testing and custom navigation wrappers).
    /// </summary>
    /// <param name="frameAdapter">The custom <see cref="INavigationFrame"/> adapter instance.</param>
    internal void SetFrameAdapter(INavigationFrame frameAdapter)
    {
        ArgumentNullException.ThrowIfNull(frameAdapter);
        _frameAdapter = frameAdapter;
    }
}
