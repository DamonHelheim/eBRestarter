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
    public static class NavigationServiceExtensions
    {
        // "this" macht es zur Extension Method für IServiceCollection
        public static IServiceCollection AddNavigationService(this IServiceCollection services)
        {
            //// 1. ViewModels & Fenster
            //// HINWEIS: Falls du diese bereits in "AddViewModels()" hast, 
            //// solltest du sie hier entfernen, um doppelte Registrierung zu vermeiden!
            //services.AddSingleton<MainViewModel>();
            //services.AddSingleton<EBRestarter>();

            // 2. NavigationService mit Factory Pattern
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

            // Wichtig für Chaining (Fluent API)
            return services;
        }
    }
}
