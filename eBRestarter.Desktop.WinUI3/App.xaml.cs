using eBRestarter.Desktop.WinUI3.DependencyInjections;
using eBRestarter.Desktop.WinUI3.Services;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using eBRestarter.Desktop.WinUI3.ViewModels;
using eBRestarter.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Devices.Display.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Globalization;
using Windows.Services.Maps;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace eBRestarter.Desktop.WinUI3
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Microsoft.UI.Xaml.Application
    {
        public static Window? MainWindoweBRestarter { get; private set; }
        public static IHost? AppHost { get; private set; } // Generic Host
        public static Microsoft.UI.Dispatching.DispatcherQueue? AppDispatcherQueue { get; private set; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();

            AppHost = CreateHostBuilder().Build();

            ApplicationLanguages.PrimaryLanguageOverride = "en-US"; // oder "de-DE"
        }

        private static IHostBuilder CreateHostBuilder() => Host.CreateDefaultBuilder() // explizit statische Klasse
                 .ConfigureAppConfiguration((ctx, cfg) =>
                 {
                     cfg.Sources.Clear();
                     cfg.SetBasePath(AppContext.BaseDirectory);
                     cfg.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                 })
                 .ConfigureServices((ctx, services) =>
                 {
                     services.AddInfrastructureServices();
                     services.AddNavigationService();
                     services.AddViewModels();
                 });

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            MainWindoweBRestarter = AppHost!.Services.GetRequiredService<EBRestarter>();
            AppDispatcherQueue = MainWindoweBRestarter.DispatcherQueue;
            MainWindoweBRestarter.Activate();
        }
    }
}
