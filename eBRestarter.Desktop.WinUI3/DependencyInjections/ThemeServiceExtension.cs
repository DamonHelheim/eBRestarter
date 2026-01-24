using eBRestarter.Desktop.WinUI3.Services;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using eBRestarter.Desktop.WinUI3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Desktop.WinUI3.DependencyInjections
{
    public static class ThemeServiceExtension
    {
        public static IServiceCollection AddThemeService(this IServiceCollection services)
        {
            // 1. ViewModels & Fenster registrieren
            services.AddTransient<IThemeService, ThemeService>();
         
            return services;
        }
    }
}
