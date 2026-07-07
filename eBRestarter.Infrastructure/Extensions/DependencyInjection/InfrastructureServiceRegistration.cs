using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Validators;
using eBRestarter.Core.Application.Ports.Outbound;
using eBRestarter.Core.Application.Ports.Outbound.Authentication;
using eBRestarter.Core.Application.Ports.Outbound.Config;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Application;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Logging;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Network;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Update;
using eBRestarter.Core.Application.Ports.Outbound.Security;
using eBRestarter.Infrastructure.Adapters.Browsers;
using eBRestarter.Infrastructure.Adapters.Http;
using eBRestarter.Infrastructure.Adapters.Outbound.API;
using eBRestarter.Infrastructure.Adapters.Outbound.API.Authentication;
using eBRestarter.Infrastructure.Adapters.Outbound.Browsers;
using eBRestarter.Infrastructure.Adapters.Outbound.Logging;
using eBRestarter.Infrastructure.Adapters.Outbound.Validators;
using eBRestarter.Infrastructure.Adapters.Outbound.WindowsOS.Authentication;
using eBRestarter.Infrastructure.Adapters.Outbound.WindowsOS.Providers;
using eBRestarter.Infrastructure.Adapters.Outbound.WindowsOS.Providers.Wrapper;
using eBRestarter.Infrastructure.Adapters.Outbound.WindowsOS.Repositories;
using eBRestarter.Infrastructure.Adapters.Outbound.WindowsOS.Repository;
using eBRestarter.Infrastructure.Adapters.Update;
using eBRestarter.Infrastructure.Adapters.Validators;
using eBRestarter.Infrastructure.Adapters.WindowsOS;
using eBRestarter.Infrastructure.Adapters.WindowsOS.Security;
using eBRestarter.Infrastructure.Adapters.Wrapper;
using eBRestarter.Infrastructure.Api;
using eBRestarter.Infrastructure.Api.Interfaces;
using eBRestarter.Infrastructure.API;
using eBRestarter.Infrastructure.Factories;
using eBRestarter.Infrastructure.Providers;
using eBRestarter.Infrastructure.Repositories.Authentication;
using eBRestarter.Infrastructure.Repositories.Config;
using eBRestarter.Infrastructure.Wrappers;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace eBRestarter.Infrastructure.Extensions.DependencyInjection;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddLogging(builder => builder.AddDebug());

        services.AddSingleton<IProcessWrapper, ProcessWrapper>();
        services.AddSingleton<IOutboundPortOsProcessControl, AdapterWindowsProcessControl>();
        services.AddSingleton<IOutboundPortFileSystem, AdapterWindowsFileSystem>();
        services.AddSingleton<IOutboundPortApplicationLifetime, AdapterWindowsApplicationLifetime>();
        services.AddSingleton<AdapterWindowsBrowserExtensionDeployment>();
        services.AddSingleton<IOutboundPortBrowserExtensionDeployment>(sp => sp.GetRequiredService<AdapterWindowsBrowserExtensionDeployment>());
        services.AddSingleton<IOutboundPortBrowserExtensionPathProvider>(sp => sp.GetRequiredService<AdapterWindowsBrowserExtensionDeployment>());
        services.AddSingleton<IOutboundPortUpdate, AdapterGitHubUpdate>();
        services.AddSingleton<IOutboundPortFileDeletion, AdapterWindowsFileDeletionService>();
        services.AddSingleton<IOutboundPortEncryption, AdapterWindowsEncryption>();
        services.AddSingleton<IOutboundPortBrowserFactory, BrowserFactory>();
        services.AddSingleton<IOutboundPortBrowserDiscoveryProvider, AdapterWindowsBrowserDiscoveryProvider>();
        services.AddSingleton<IOutboundPortSystemInfoProvider, AdapterWindowsSystemInfoProvider>();
        services.AddSingleton<IOutboundPortEVisitorApiProvider, AdapterEVisitorApiProvider>();

        services.AddSingleton<IOutboundPortProcessInfoProvider, AdapterProcessInfoProvider>();
        services.AddSingleton<IOutboundPortAppPathProvider, AdapterWindowsAppPathProvider>();
        services.AddSingleton<IOutboundPortHardwareInfoProvider>(provider => provider.GetRequiredService<AdapterWmiHardwareProvider>());
        services.AddSingleton<IOutboundPortOsEditionProvider>(provider => provider.GetRequiredService<AdapterWmiHardwareProvider>());
        services.AddSingleton<IOutboundPortNetworkProvider, AdapterWindowsNetworkProvider>();
        services.AddSingleton<IInboundPortNetworkInfoProvider, NetworkInfoProvider>();
        services.AddSingleton<IInboundPortOsAppPathProvider, WindowsAppPathProvider>();
        services.AddSingleton<AdapterWmiHardwareProvider>();

        services.AddTransient<AdapterChromeBrowser>();
        services.AddTransient<AdapterFirefoxBrowser>();
        services.AddTransient<AdapterEdgeBrowser>();
        services.AddTransient<AdapterBraveBrowser>();
        services.AddTransient<AdapterVivaldiBrowser>();

        services.AddSingleton<IOutboundPortHttpDownload, AdapterHttpClientDownloadHandler>();

        services.AddSingleton<IRestClient, RestSharpClient>();
        services.AddSingleton<IOutboundPortApiAuthenticationProvider, AdapterEVisitorApiAuthenticationProvider>();
        services.AddSingleton<IOutboundPortActiveDirectoryProvider, AdapterWindowsActiveDirectoryProvider>();
        services.AddSingleton<IOutboundPortCredentialValidationProvider, AdapterWindowsCredentialValidationProvider>();
        services.AddSingleton<IOutboundPortAppVersionInfoProvider, AdapterWindowsAppVersionInfoProvider>();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IOutboundPortSystemConfigurationRepository, AdapterWindowsRegistryRepository>();
        services.AddSingleton<IOutboundPortAutoStartRepository, AdapterWindowsStartupRepository>();
        services.AddSingleton<IOutboundPortBrowserConfigRepository, AdapterWindowsStartupRepository>();
        services.AddSingleton<IOutboundPortOsAutoLogonRepository, AdapterWindowsAutoLogonRepository>();
        services.AddSingleton<EVRestarterConfigRepository>();

        services.AddSingleton<IOutboundPortEVisitorConfigRepository>(provider =>

            new EncryptedEVisitorConfigRepositoryDecorator(

                provider.GetRequiredService<EVRestarterConfigRepository>(),
                provider.GetRequiredService<IOutboundPortEncryption>(),
                provider.GetRequiredService<ILogger<EncryptedEVisitorConfigRepositoryDecorator>>()

            ));

        services.AddSingleton<IOutboundPortCredentialStoreRepository, JsonCredentialStoreRepository>();

        services.AddValidatorsFromAssemblyContaining<ConfigureAutoLogonValidator>();
        services.AddTransient(typeof(IInboundPortApplicationValidator<>), typeof(AdapterFluentValidation<>));
        services.AddSingleton(typeof(IOutboundPortApplicationLogger<>), typeof(AdapterMicrosoftExtensionsLogger<>));

        return services;
    }
}







































