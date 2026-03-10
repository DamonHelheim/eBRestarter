using eBRestarter.Core.Application.Interfaces;
using System.Reflection;

namespace eBRestarter.Infrastructure.Services;

/// <summary>
/// Implementiert den IAppInfoService für Windows.
/// </summary>
public class WindowsAppVersionInfoService : IAppVersionInfoService
{
    public string GetAppVersion()
    {
        // Da die App nun Unpackaged (klassische .exe) ist,
        // lesen wir die Version direkt aus den Metadaten der Assembly aus.
        var assemblyVersion = Assembly.GetEntryAssembly()?.GetName().Version;

        if (assemblyVersion != null)
        {
            return $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}.{assemblyVersion.Revision}";
        }

        return "1.0.0.0"; // Fallback
    }
}