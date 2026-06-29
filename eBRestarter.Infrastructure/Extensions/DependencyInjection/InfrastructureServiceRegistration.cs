using eBRestarter.Core.Application.Handlers;
using eBRestarter.Core.Application.Parsers;
using eBRestarter.Core.Application.Ports.Outbound;
using eBRestarter.Core.Application.Ports.Outbound.Application;
using eBRestarter.Core.Application.Ports.Outbound.Authentication;
using eBRestarter.Core.Application.Ports.Outbound.Browser;
using eBRestarter.Core.Application.Ports.Outbound.Config;
using eBRestarter.Core.Application.Ports.Outbound.Network;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Core.Application.Ports.Outbound.Scheduling;
using eBRestarter.Core.Application.Ports.Outbound.Security;
using eBRestarter.Core.Application.Ports.Outbound.Update;
using eBRestarter.Core.Application.Providers;
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace eBRestarter.Infrastructure.Extensions.DependencyInjection;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddLogging(builder => builder.AddDebug());

        services.AddSingleton<IProcessWrapper, ProcessWrapper>();
        services.AddSingleton<IOsProcessControlPort, WindowsProcessControlAdapter>();
        services.AddSingleton<IFileSystemPort, WindowsFileSystemAdapter>();
        services.AddSingleton<IApplicationLifetimePort, WindowsApplicationLifetimeAdapter>();
        services.AddSingleton<IComputerRestartSchedulerPort, ComputerRestartHandler>();
        services.AddSingleton<IBrowserExtensionDeploymentPort, WindowsBrowserExtensionDeploymentAdapter>();
        services.AddSingleton<IUpdatePort, GitHubUpdateAdapter>();
        services.AddSingleton<IFileDeletionPort, WindowsFileDeletionAdapter>();
        services.AddSingleton<IEncryptionPort, WindowsEncryptionAdapter>();

        services.AddSingleton<ISystemInfoPort, WindowsSystemInfoAdapter>();
        services.AddSingleton<IProcessInfoPort, ProcessInfoAdapter>();
        services.AddSingleton<IAppPathPort, WindowsAppPathProviderAdapter>();
        services.AddSingleton<IHardwareInfoPort>(provider => provider.GetRequiredService<WmiHardwareProviderAdapter>());
        services.AddSingleton<IOsEditionPort>(provider => provider.GetRequiredService<WmiHardwareProviderAdapter>());
        services.AddSingleton<INetworkProviderPort, WindowsNetworkProviderAdapter>();
        services.AddSingleton<INetworkInfoPort, NetworkInfoProvider>();
        services.AddSingleton<IOsPathProviderPort, WindowsOsPathProvider>();
        services.AddSingleton<IAppInfoPort, AppInfoProviderAdapter>();
        services.AddSingleton<WmiHardwareProviderAdapter>();

        services.AddTransient<ChromeBrowser>();
        services.AddTransient<FirefoxBrowser>();
        services.AddTransient<EdgeBrowser>();
        services.AddTransient<BraveBrowser>();
        services.AddTransient<VivaldiBrowser>();

        services.AddSingleton<IBrowserDownloadPort, HttpClientDownloadHandlerAdapter>();

        services.AddSingleton<IBrowserFactoryPort, BrowserFactory>();
        services.AddSingleton<IBrowserDiscoveryPort, WindowsBrowserDiscoveryProviderAdapter>();

        services.AddSingleton<IRestClientPort, RestSharpClientAdapter>();
        services.AddSingleton<IApiAuthenticationPort, EVisitorApiAuthenticationAdapter>();
        services.AddSingleton<IEVisitorApiProviderPort, EVisitorApiProviderAdapter>();
        services.AddSingleton<IEVisitorApiResponseParser, EVisitorApiResponseParser>();
        services.AddSingleton<IActiveDirectoryPort, WindowsActiveDirectoryProviderAdapter>();
        services.AddSingleton<ICredentialValidationPort, WindowsCredentialValidationAdapter>();
        services.AddSingleton<IAppVersionInfoPort, WindowsAppVersionInfoProviderAdapter>();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ISettingsPort, WindowsRegistryRepository>();
        services.AddSingleton<IAutoStartPort, WindowsStartupRepository>();
        services.AddSingleton<IBrowserConfigPort, WindowsStartupRepository>();
        services.AddSingleton<IOsAutoLogonPort, WindowsAutoLogonRepository>();
        services.AddSingleton<EVRestarterConfigRepository>();

        services.AddSingleton<IEVisitorConfigPort>(provider =>

            new EncryptedEVisitorConfigRepositoryDecorator(

                provider.GetRequiredService<EVRestarterConfigRepository>(),
                provider.GetRequiredService<IEncryptionPort>(),
                provider.GetRequiredService<ILogger<EncryptedEVisitorConfigRepositoryDecorator>>()

            ));

        services.AddSingleton<ICredentialStorePort, JsonCredentialStoreRepository>();



        return services;
    }
}







































