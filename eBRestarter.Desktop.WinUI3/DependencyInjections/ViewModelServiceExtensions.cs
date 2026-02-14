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
        // =========================================================
        // 1. PUBLIC & PROTECTED METHODS (API / Extension)
        // =========================================================
        #region PublicAndProtectedMethods

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
