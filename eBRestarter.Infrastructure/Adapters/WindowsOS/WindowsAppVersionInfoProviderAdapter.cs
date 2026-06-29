using eBRestarter.Core.Application.Ports.Outbound.Application;
using System.Reflection;

namespace eBRestarter.Infrastructure.Adapters.WindowsOS;

/// <summary>
/// Implements the IAppVersionInfoPort for Windows.
/// </summary>
public sealed class WindowsAppVersionInfoProviderAdapter : IAppVersionInfoPort
{
    public string RetrieveAppVersion()
    {
        // Since the application is now Unpackaged (classic standalone .exe distribution format),
        // we extract the version metadata attributes directly from the entry assembly context.
        var assemblyVersion = Assembly.GetEntryAssembly()?.GetName().Version;

        if (assemblyVersion is not null)
        {
            return $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}.{assemblyVersion.Revision}";
        }

        return new Version(1, 0, 0, 0).ToString(); // Fallback threshold execution
    }
}




