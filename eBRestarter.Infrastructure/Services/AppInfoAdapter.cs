using eBRestarter.Core.Application.Interfaces;
using System.Diagnostics;
using System.Reflection;

namespace eBRestarter.Infrastructure.Services;

public class AppInfoAdapter : IAppInfoUseCase
{
    public string RetrieveAppVersion()
    {
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
        return fvi.FileVersion ?? "1.0.0";
    }
}
