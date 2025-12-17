using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Desktop.WinUI3.Services
{
    /// <summary>
    /// Eine Wrapper-Klasse (Adapter), die das <see cref="INavigationFrame"/>-Interface implementiert
    /// und die Aufrufe an einen echten WinUI-<see cref="Frame"/> weiterleitet.
    /// <br/>
    /// Diese Klasse wird zur Laufzeit der App verwendet.
    /// </summary>
    public class WinUIFrameAdapter : INavigationFrame
    {
        private readonly Frame _frame;

        /// <summary>
        /// Initialisiert eine neue Instanz des <see cref="WinUIFrameAdapter"/>.
        /// </summary>
        /// <param name="frame">Der echte WinUI-Frame, der gekapselt werden soll.</param>
        public WinUIFrameAdapter(Frame frame) => _frame = frame;

        /// <inheritdoc />
        public bool CanGoBack => _frame.CanGoBack;

        /// <inheritdoc />
        public object Content => _frame.Content;

        /// <inheritdoc />
        public void GoBack() => _frame.GoBack();

        /// <inheritdoc />
        public bool Navigate(Type sourcePageType, object parameter, NavigationTransitionInfo infoOverride)
        {
            return _frame.Navigate(sourcePageType, parameter, infoOverride);
        }
    }
}
