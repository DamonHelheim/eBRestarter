using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using eBRestarter.Desktop.WinUI3.ViewModels;
using eBRestarter.Desktop.WinUI3.Views.Pages;
using eBRestarter.Desktop.WinUI3.Views.UserControls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Desktop.WinUI3.DependencyInjections
{
    public static class ViewModelServiceExtensions
    {
        // Das "this" vor dem Parameter macht es zur Extension Method
        public static IServiceCollection AddViewModels(this IServiceCollection services)
        {
            // 1. ViewModels & Fenster registrieren
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<EBRestarter>();

            services.AddSingleton<ViewModelRestarterProperties>();
            // Hinweis: Pages registriert man oft als Transient, 
            // aber Singleton ist okay, wenn sie den Zustand behalten sollen.
            services.AddSingleton<P_CommonOverview>();

            services.AddSingleton<InstalledBrowsersViewModel>();
            services.AddSingleton<UC_InstalledBrowsers>();

            services.AddSingleton<ViewModelOptions>();
            services.AddSingleton<UC_Options>();

            services.AddSingleton<ViewModelInfocenter>();
            services.AddSingleton<UC_Infocenter>();

            services.AddSingleton<ViewModelAbout>();

            services.AddTransient<NetworkTrafficViewModel>();
            services.AddTransient<ViewModelDeleteBrowserContent>();

            // Rückgabe von "services" ermöglicht Chaining (services.AddX().AddY())
            return services;
        }
    }
}
