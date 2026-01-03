using eBRestarter.Core.Application.Facade;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Application.Interfaces.RestClient;
using eBRestarter.Infrastructure.Browsers;
using eBRestarter.Infrastructure.Factories;
using eBRestarter.Infrastructure.Services;
using eBRestarter.Infrastructure.Services.Config;
using eBRestarter.Infrastructure.Services.RestSharp;
using eBRestarter.Infrastructure.Services.WindowsOS;
using eBRestarter.Infrastructure.Services.WindowsOS.Security;
using eBRestarter.Infrastructure.Wrapper;
using eBRestarter.Infrastructure.Wrapper.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            // HIER: Standard Logging registrieren
            // Das registriert die notwendigen Services (ILoggerFactory, ILogger<>),
            // damit Dependency Injection in den Klassen funktioniert.
            services.AddLogging(builder =>
            {
                // Optional: Standard-Provider hinzufügen (falls du z.B. Debug-Output willst)
                // Wenn du Serilog im Host (App.xaml.cs) konfigurierst, überschreibt Serilog das meistens eh.

                // builder.AddConsole(); // Schreibt in die Konsole
                builder.AddDebug();      // Schreibt ins Visual Studio "Ausgabe"-Fenster
            });

            // Die Windows-Services registrieren
#pragma warning disable CA1416 // Plattformkompatibilität überprüfen

            // 1. Erst die Wrapper registrieren (oft als Transient oder Singleton)
            // Wir registrieren den "echten" Wrapper für die Laufzeit der App.
            services.AddTransient<IProcessWrapper, RealProcessWrapper>();
            services.AddSingleton<IWindowsProcessControlService, WindowsProcessService>();
            services.AddSingleton<IWindowsSystemInfoService, WindowsSystemInfoService>();

            // Registrierung der Wrapper
            services.AddTransient<IProcessInfoService, ProcessInfoService>(); 
            services.AddTransient<IWindowsRegistryService, WindowsRegistryService>();
            services.AddTransient<IWindowsFileSystemService, WindowsFileSystemService>();
            services.AddSingleton<IWindowsStartupManagerService, WindowsStartupService>();

            services.AddTransient<IWindowsAutoLogonService, WindowsAutoLogonService>();

#pragma warning restore CA1416 // Plattformkompatibilität überprüfen

            // 1. Die konkrete Klasse als Singleton registrieren
            services.AddSingleton<WindowsWmiHardwareService>();

            // 2. Das erste Interface anfordern -> leitet weiter an die bereits erstellte Instanz
            services.AddSingleton<IHardwareInfoService>(provider => provider.GetRequiredService<WindowsWmiHardwareService>());

            // 3. Das zweite Interface anfordern -> leitet weiter an dieselbe Instanz
            services.AddSingleton<IOsEditionService>(provider => provider.GetRequiredService<WindowsWmiHardwareService>());

            // NEU: Die Facade registrieren
            services.AddSingleton<IOperatingSystemFacade, OperatingSystemFacade>();

            services.AddSingleton<IWindowsNetworkInfoService, WindowsNetworkInfoService>();

            // 2. Browser (Transient, damit sie bei jedem Factory-Call frisch sind)
            services.AddTransient<ChromeBrowser>();
            services.AddTransient<FirefoxBrowser>();
            services.AddTransient<EdgeBrowser>();
            services.AddTransient<BraveBrowser>();

            // 3. Factory
            services.AddSingleton<IBrowserFactory, BrowserFactory>();

            services.AddSingleton<IBrowserService, WindowsBrowserService>();

            services.AddSingleton<IBrowserDownloadService, HttpClientDownloadService>();
            // Registrierung des Interfaces mit der Implementierung
            services.AddTransient<IRestClientService, RestSharpClientService>();

            services.AddSingleton<IPathService, WindowsPathService>();

            services.AddSingleton<IEVisitorConfigService, EVRestarterConfigService>();

            services.AddSingleton<IAppInfoService, AppInfoService>();

            return services;
        }
    }
}
