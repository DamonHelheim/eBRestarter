using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Core.Application.Ports.Outbound.Application;
using System.Diagnostics;
using System.Reflection;

namespace eBRestarter.Infrastructure.Adapters;

public sealed class AppInfoProviderAdapter : IAppInfoPort
{
    public string RetrieveAppVersion()
    {
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
        return fvi.FileVersion ?? "1.0.0";
    }
}





