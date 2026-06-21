using eBRestarter.Core.Application.Interfaces;
using System.Reflection;

namespace eBRestarter.Infrastructure.Services;

/// <summary>
/// Implementiert den IAppInfoService für Windows.
/// </summary>
public class WindowsAppVersionInfoHandler : IAppVersionInfoUseCase
{
    public string RetrieveAppVersion()
    {
        // Da die App nun Unpackaged (klassische .exe) ist,
        // lesen wir die Version direkt aus den Metadaten der Assembly aus.
        var assemblyVersion = Assembly.GetEntryAssembly()?.GetName().Version;

        if (assemblyVersion is not null)
        {
            return $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}.{assemblyVersion.Revision}";
        }

        return new Version(1, 0, 0, 0).ToString(); // Fallback
    }
}
