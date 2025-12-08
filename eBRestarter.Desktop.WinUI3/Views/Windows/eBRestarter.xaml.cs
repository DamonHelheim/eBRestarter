using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using eBRestarter.Desktop.WinUI3.ViewModels;
using eBRestarter.Desktop.WinUI3.Views.Pages;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace eBRestarter.Desktop.WinUI3
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EBRestarter : Window
    {
        private readonly INavigationService _navigationService;
        private readonly NavigationTransitionInfo _defaultTransition = new DrillInNavigationTransitionInfo();

        // Routen-Map für Tags aus dem NavigationView
        private static readonly Dictionary<string, Type> _routes = new()
        {
            ["CommonOverview"] = typeof(P_CommonOverview),
            ["RestarterProperties"] = typeof(P_RestarterProperties),
            ["Options"] = typeof(P_Options),
            ["Infocenter"] = typeof(P_Infocenter),
            ["Settings"] = typeof(P_Settings)
        };

        public EBRestarter(INavigationService navigationService, MainViewModel mainViewModel)
        {
            InitializeComponent();
            ConfigureTitleBar();
            _navigationService = navigationService;

            // Frame an NavigationService "anhängen"
            _navigationService.AttachFrame(NavigationFrame);

            // Optional: DataContext aus DI
            // DataContext = App.AppHost!.Services.GetRequiredService<MainViewModel>();

            (this.Content as FrameworkElement)!.DataContext = mainViewModel;

            // Startseite setzen
            _navigationService.Navigate(typeof(P_CommonOverview), new DrillInNavigationTransitionInfo());
        }

        private void ConfigureTitleBar()
        {
            // AppWindow aus dem Window holen (ab Windows App SDK 1.3+)
            var appWindow = this.AppWindow;
            var titleBar = appWindow.TitleBar;

            // Inhalt in die Titelleiste hineinziehen
            titleBar.ExtendsContentIntoTitleBar = true;

            // Farben an deinen Hintergrund anpassen
            var bg = Color.FromArgb(255, 32, 37, 54); // Fix: ColorHelper entfernt, stattdessen Microsoft.UI.Xaml.Media.Color

            titleBar.BackgroundColor = bg;
            titleBar.InactiveBackgroundColor = bg;
            titleBar.ButtonBackgroundColor = bg;
            titleBar.ButtonInactiveBackgroundColor = bg;
            titleBar.ButtonForegroundColor = Colors.White;
            titleBar.ButtonInactiveForegroundColor = Colors.Gray;

            // Wenn du eine eigene XAML-Titlebar benutzt, dann zusätzlich:
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(null);
        }

        private void AppTitleBar_BackRequested(TitleBar sender, object args)
        {
            if (NavigationFrame.CanGoBack)
            {
                NavigationFrame.GoBack();
            }
        }

        private void AppTitleBar_PaneToggleRequested(TitleBar sender, object args)
        {
            NavView.IsPaneOpen = !NavView.IsPaneOpen;
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer is NavigationViewItem nvi
                && nvi.Tag is string tag
                && _routes.TryGetValue(tag, out var pageType))
            {
                _navigationService.Navigate(
                    pageType,
                    parameter: null,
                    infoOverride: _defaultTransition
                );
            }
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                // Falls du später eigene Settings-Page hast
                // _navigationService.Navigate(typeof(P_Options));
                return;
            }

            if (args.InvokedItemContainer is NavigationViewItem nvi
                && nvi.Tag is string tag
                && _routes.TryGetValue(tag, out var pageType))
            {
                _navigationService.Navigate(pageType, new DrillInNavigationTransitionInfo());
            }
        }
    }
}

