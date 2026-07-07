using eBRestarter.Core.Application.Handlers;
// Removed parser
using eBRestarter.Core.Application.Ports.Outbound;
using eBRestarter.Core.Application.Ports.Outbound.Application;
using eBRestarter.Core.Application.Ports.Outbound.Authentication;
using eBRestarter.Core.Application.Ports.Outbound.Browser;
using eBRestarter.Core.Application.Ports.Outbound.Config;
using eBRestarter.Core.Application.Ports.Outbound.Network;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Core.Application.Ports.Inbound.Services;
using eBRestarter.Core.Application.Ports.Inbound.Providers;
using eBRestarter.Core.Application.Ports.Outbound.Security;
using eBRestarter.Core.Application.Ports.Outbound.Update;
using eBRestarter.Infrastructure.Adapters;
using eBRestarter.Infrastructure.Adapters.Authentication;
using eBRestarter.Infrastructure.Adapters.Browsers;
using eBRestarter.Infrastructure.Adapters.Http;
using eBRestarter.Infrastructure.Adapters.Providers.WindowsOS;
using eBRestarter.Infrastructure.Adapters.RestSharp;
using eBRestarter.Infrastructure.Adapters.Update;
using eBRestarter.Infrastructure.Adapters.WindowsOS;
using eBRestarter.Infrastructure.Adapters.WindowsOS.Security;
using eBRestarter.Infrastructure.Adapters.Wrapper;
using eBRestarter.Infrastructure.Factories;
using eBRestarter.Infrastructure.Network;
using eBRestarter.Infrastructure.Repositories.Authentication;
using eBRestarter.Infrastructure.Repositories.Config;
using eBRestarter.Core.Application.Ports.Inbound.Validators;
using eBRestarter.Core.Application.Ports.Outbound.Logging;
using eBRestarter.Infrastructure.Adapters.Logging;
using eBRestarter.Infrastructure.Adapters.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using eBRestarter.Infrastructure.Providers;

namespace eBRestarter.Infrastructure.Extensions.DependencyInjection;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddLogging(builder => builder.AddDebug());

        services.AddSingleton<IProcessWrapper, ProcessWrapper>();
        services.AddSingleton<IOsProcessControlOutboundPort, WindowsProcessControlAdapter>();
        services.AddSingleton<IFileSystemOutboundPort, WindowsFileSystemAdapter>();
        services.AddSingleton<IApplicationLifetimeOutboundPort, WindowsApplicationLifetimeAdapter>();
        services.AddSingleton<WindowsBrowserExtensionDeploymentAdapter>();
        services.AddSingleton<IBrowserExtensionDeploymentOutboundPort>(sp => sp.GetRequiredService<WindowsBrowserExtensionDeploymentAdapter>());
        services.AddSingleton<IBrowserExtensionPathProviderOutboundPort>(sp => sp.GetRequiredService<WindowsBrowserExtensionDeploymentAdapter>());
        services.AddSingleton<IUpdateOutboundPort, GitHubUpdateAdapter>();
        services.AddSingleton<IFileDeletionOutboundPort, WindowsFileDeletionServiceAdapter>();
        services.AddSingleton<IEncryptionOutboundPort, WindowsEncryptionAdapter>();
        services.AddSingleton<IBrowserFactoryOutboundPort, BrowserFactory>();
        services.AddSingleton<IBrowserDiscoveryProviderOutboundPort, WindowsBrowserDiscoveryProvider>();
        services.AddSingleton<ISystemInfoProviderOutboundPort, WindowsSystemInfoProvider>();
        services.AddSingleton<IEVisitorApiProviderOutboundPort, EVisitorApiProvider>();

        services.AddSingleton<IProcessInfoProviderOutboundPort, ProcessInfoProvider>();
        services.AddSingleton<IAppPathProviderOutboundPort, Adapters.Providers.WindowsOS.WindowsAppPathProvider>();
        services.AddSingleton<IHardwareInfoProviderOutboundPort>(provider => provider.GetRequiredService<WmiHardwareProvider>());
        services.AddSingleton<IOsEditionProviderOutboundPort>(provider => provider.GetRequiredService<WmiHardwareProvider>());
        services.AddSingleton<INetworkProviderOutboundPort, WindowsNetworkProvider>();
        services.AddSingleton<IInboundPortNetworkInfoProvider, NetworkInfoProvider>();
        services.AddSingleton<IInboundPortOsAppPathProvider, Providers.WindowsAppPathProvider>();
        services.AddSingleton<IAppInfoProviderOutboundPort, AppInfoProvider>();
        services.AddSingleton<WmiHardwareProvider>();

        services.AddTransient<ChromeBrowser>();
        services.AddTransient<FirefoxBrowser>();
        services.AddTransient<EdgeBrowser>();
        services.AddTransient<BraveBrowser>();
        services.AddTransient<VivaldiBrowser>();

        services.AddSingleton<IHttpDownloadOutboundPort, HttpClientDownloadHandlerAdapter>();

        services.AddSingleton<IRestClientPort, RestSharpClientAdapter>();
        services.AddSingleton<IApiAuthenticationProviderOutboundPort, EVisitorApiAuthenticationProvider>();
        services.AddSingleton<IActiveDirectoryProviderOutboundPort, WindowsActiveDirectoryProvider>();
        services.AddSingleton<ICredentialValidationProviderOutboundPort, WindowsCredentialValidationProvider>();
        services.AddSingleton<IAppVersionInfoProviderOutboundPort, WindowsAppVersionInfoProvider>();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ISettingsRepositoryOutboundPort, WindowsRegistryRepository>();
        services.AddSingleton<IAutoStartRepositoryOutboundPort, WindowsStartupRepository>();
        services.AddSingleton<IBrowserConfigRepositoryOutboundPort, WindowsStartupRepository>();
        services.AddSingleton<IOsAutoLogonRepositoryOutboundPort, WindowsAutoLogonRepository>();
        services.AddSingleton<EVRestarterConfigRepository>();

        services.AddSingleton<IEVisitorConfigRepositoryOutboundPort>(provider =>

            new EncryptedEVisitorConfigRepositoryDecorator(

                provider.GetRequiredService<EVRestarterConfigRepository>(),
                provider.GetRequiredService<IEncryptionOutboundPort>(),
                provider.GetRequiredService<ILogger<EncryptedEVisitorConfigRepositoryDecorator>>()

            ));

        services.AddSingleton<ICredentialStoreRepositoryOutboundPort, JsonCredentialStoreRepository>();

        services.AddValidatorsFromAssemblyContaining<ConfigureAutoLogonValidator>();
        services.AddTransient(typeof(IInboundPortApplicationValidator<>), typeof(FluentValidationAdapter<>));
        services.AddSingleton(typeof(IApplicationLoggerOutboundPort<>), typeof(MicrosoftExtensionsLoggerAdapter<>));

        return services;
    }
}







































