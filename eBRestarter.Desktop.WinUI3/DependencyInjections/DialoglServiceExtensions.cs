using eBRestarter.Desktop.WinUI3.Services;
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
    /// Extension methods to register dialog services with the DI container.
    /// </summary>
    public static class DialoglServiceExtensions
    {
        // =========================================================
        // 1. PUBLIC METHODS (API / Extension)
        // =========================================================
        #region PublicMethods

        public static IServiceCollection AddDialoglServiceExtensions(this IServiceCollection services)
        {
            services.AddTransient<IDialogService, DialogService>();
            return services;
        }

        #endregion
    }
}
