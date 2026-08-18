using System;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

using eBRestarter.Desktop.WinUI3.BehavioralComponents.Services.Interfaces;

namespace eBRestarter.Desktop.WinUI3.BehavioralUIComponents.Navigation;

/// <summary>
/// Adapter that implements <see cref="INavigationFrame"/> and delegates navigation commands to a WinUI3 <see cref="Frame"/>.
/// </summary>
/// <param name="frame">The underlying WinUI <see cref="Frame"/> instance.</param>
public sealed class WinUIFrameNavigator(
    Frame frame) : INavigationFrame
{
    private readonly Frame _frame = frame ?? throw new ArgumentNullException(nameof(frame));

    /// <inheritdoc />
    public bool CanGoBack => _frame.CanGoBack;

    /// <inheritdoc />
    public object Content => _frame.Content;

    /// <inheritdoc />
    public void GoBack() => _frame.GoBack();

    /// <inheritdoc />
    public bool Navigate(
        Type sourcePageType,
        object parameter,
        NavigationTransitionInfo infoOverride)
    {
        ArgumentNullException.ThrowIfNull(sourcePageType);

        return _frame.Navigate(sourcePageType, parameter, infoOverride);
    }
}
