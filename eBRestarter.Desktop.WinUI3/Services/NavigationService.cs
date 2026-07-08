using eBRestarter.Desktop.WinUI3.Adapters;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;

namespace eBRestarter.Desktop.WinUI3.Services;

/// <summary>
/// Central navigation service: registers routes and navigates via an abstracted frame adapter.
/// </summary>
public sealed class NavigationService : INavigationService
{

    private readonly Dictionary<string, Type> _pages = [];

    private INavigationFrame? _frameAdapter;

    public void AttachFrame(Frame frame)
    {
        _frameAdapter = new WinUIFrameNavigator(frame);
    }

    internal void SetFrameAdapter(INavigationFrame frameAdapter)
    {
        _frameAdapter = frameAdapter;
    }

    public bool NavigateTo(string key, object parameter = null!, NavigationTransitionInfo transitionInfo = null!)
    {
        if (_frameAdapter == null) { return false; }

        if (_pages.TryGetValue(key, out var pageType))
        {
            if (_frameAdapter.Content?.GetType() == pageType) { return false; }

            return _frameAdapter.Navigate(pageType, parameter, transitionInfo);
        }

        return false;
    }

    public void RegisterRoute(string key, Type pageType)
    {
        _pages.TryAdd(key, pageType);
    }

}
