using System;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

using eBRestarter.Desktop.WinUI3.BehavioralComponents.Services.Interfaces;

namespace eBRestarter.Desktop.WinUI3.BehavioralUIComponents.Navigation;

/// <summary>
/// Adapter that implements <see cref="INavigationFrame"/> and delegates navigation commands to a WinUI3 <see cref="Frame"/>.
/// </summary>
public sealed class WinUIFrameNavigator(
    Frame frame) : INavigationFrame
{
    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 4: Komplexe Typen / Framework-Objekte ──
    private readonly Frame _frame = frame ?? throw new ArgumentNullException(nameof(frame));


    // ═══════════════════════════════════════════════════════
    //  6. Properties
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Gets a value indicating whether there is at least one entry in back navigation history.
    /// </summary>
    public bool CanGoBack => _frame.CanGoBack;

    /// <summary>
    /// Gets the current content displayed in the underlying frame container.
    /// </summary>
    public object Content => _frame.Content;


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Navigates to the most recent item in back navigation history.
    /// </summary>
    public void GoBack() => _frame.GoBack();

    /// <summary>
    /// Navigates asynchronously to the specified page type with optional parameter and transition override.
    /// </summary>
    /// <param name="sourcePageType">The target page type.</param>
    /// <param name="parameter">Optional parameter passed to the destination page.</param>
    /// <param name="infoOverride">Optional transition animation override.</param>
    /// <returns><c>true</c> if navigation succeeded; otherwise, <c>false</c>.</returns>
    public bool Navigate(
        Type sourcePageType,
        object parameter,
        NavigationTransitionInfo infoOverride)
    {
        ArgumentNullException.ThrowIfNull(sourcePageType);

        return _frame.Navigate(sourcePageType, parameter, infoOverride);
    }
}
