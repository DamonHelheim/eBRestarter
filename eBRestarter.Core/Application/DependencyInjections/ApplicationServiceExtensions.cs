using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Application.DependencyInjections
{
    public static class ApplicationServiceExtensions
    {
        // Das "this" vor dem Parameter macht es zur Extension Method
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddSingleton<IComputerRestartScheduler, ComputerRestartScheduler>();
            return services;
        }
    }
}
