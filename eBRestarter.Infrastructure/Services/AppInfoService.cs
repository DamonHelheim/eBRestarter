using eBRestarter.Core.Application.Interfaces;
using System.Diagnostics;
using System.Reflection;

namespace eBRestarter.Infrastructure.Services;

public class AppInfoService : IAppInfoService
{
    public string GetAppVersion()
    {
        // WinUI 3 Apps nutzen oft Package.Current.Id.Version,
        // aber für Desktop-Apps funktioniert Reflection weiterhin gut:
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
        return fvi.FileVersion ?? "1.0.0";
    }
}