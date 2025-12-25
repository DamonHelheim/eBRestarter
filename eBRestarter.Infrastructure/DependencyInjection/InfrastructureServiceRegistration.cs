using eBRestarter.Application.Facade;
using eBRestarter.Application.Facade.Interfaces;
using eBRestarter.Application.Services.Ports.Interfaces;
using eBRestarter.Infrastructure.Services.WindowsOS;
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
            services.AddSingleton<IProcessControlService, WindowsProcessService>();
            services.AddSingleton<ISystemInfoService, WindowsSystemInfoService>();

            // Registrierung der Wrapper
            services.AddTransient<IProcessInfoService, ProcessInfoService>();
            services.AddTransient<IRegistryService, RegistryService>();
            services.AddSingleton<IStartupManagerService, WindowsStartupService>();
#pragma warning restore CA1416 // Plattformkompatibilität überprüfen

            // NEU: Die Facade registrieren
            services.AddSingleton<IOperatingSystemFacade, OperatingSystemFacade>();

            return services;
        }
    }
}
