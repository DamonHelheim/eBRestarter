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
    public static class DialoglServiceExtensions
    {
        // Das "this" vor dem Parameter macht es zur Extension Method
        public static IServiceCollection AddDialoglServiceExtensions(this IServiceCollection services)
        {
            services.AddTransient<IDialogService, DialogService>();
            return services;
        }
    }
}
