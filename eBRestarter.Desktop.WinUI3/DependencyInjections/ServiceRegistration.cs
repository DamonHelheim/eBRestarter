using eBRestarter.Desktop.WinUI3.Services;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using eBRestarter.Desktop.WinUI3.ViewModels;
using eBRestarter.Desktop.WinUI3.Views.Pages;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.DependencyInjections
{
    public static class ServiceRegistration
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            // 1. ViewModels & Fenster normal registrieren
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<EBRestarter>();

            // 2. NavigationService mit KONFIGURATION registrieren (Factory Pattern)
            // Wir übergeben eine Funktion (Lambda), die ausgeführt wird, wenn der Service zum ersten Mal gebraucht wird.
            services.AddSingleton<INavigationService>(provider =>
            {
                // Instanz erzeugen
                var navService = new NavigationService();

                // Routen registrieren
                navService.RegisterRoute("CommonOverview", typeof(P_CommonOverview));
                navService.RegisterRoute("RestarterProperties", typeof(P_RestarterProperties));
                navService.RegisterRoute("Options", typeof(P_Options));
                navService.RegisterRoute("Infocenter", typeof(P_Infocenter));
                navService.RegisterRoute("Settings", typeof(P_Settings));

                // Den fertig konfigurierten Service zurückgeben
                return navService;
            });
        }
    }
}
