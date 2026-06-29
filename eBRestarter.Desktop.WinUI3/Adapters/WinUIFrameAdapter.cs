using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;

namespace eBRestarter.Desktop.WinUI3.Adapters;

/// <summary>
/// Adapter that implements <see cref="INavigationFrame"/> and delegates to a WinUI <see cref="Frame"/>.
/// </summary>
public sealed class WinUIFrameAdapter(Frame frame) : INavigationFrame
{
    private readonly Frame _frame = frame;

    public bool CanGoBack => _frame.CanGoBack;
    public object Content => _frame.Content;

    public void GoBack() => _frame.GoBack();

    public bool Navigate(Type sourcePageType, object parameter, NavigationTransitionInfo infoOverride)
    {
        return _frame.Navigate(sourcePageType, parameter, infoOverride);
    }
}
