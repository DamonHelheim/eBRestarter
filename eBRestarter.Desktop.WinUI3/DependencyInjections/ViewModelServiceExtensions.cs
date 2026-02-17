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
    /// <summary>
    /// Extension methods to register ViewModels and main window with the DI container.
    /// </summary>
    public static class ViewModelServiceExtensions
    {
        // =========================================================
        // 1. PUBLIC METHODS (API / Extension)
        // =========================================================
        #region PublicMethods

        public static IServiceCollection AddViewModels(this IServiceCollection services)
        {
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<EBRestarter>();
            services.AddSingleton<ViewModelRestartTask>();
            services.AddSingleton<ViewModelRestarterProperties>();
            services.AddSingleton<ViewModelGeneralOverview>();
            services.AddSingleton<ViewModelInstalledBrowsers>();
            services.AddSingleton<ViewModelOptions>();
            services.AddSingleton<ViewModelInfocenter>();
            services.AddSingleton<ViewModelAbout>();

            services.AddTransient<ViewModelNetworkTraffic>();
            services.AddTransient<ViewModelDeleteBrowserContent>();
            services.AddTransient<ViewModelInstallAddOn>();
            services.AddTransient<ViewModelActivateApi>();
            services.AddTransient<ViewModelImportApi>();
            services.AddTransient<ViewModelTurnOffEdgeStartupBoost>();

            return services;
        }

        #endregion
    }
}
