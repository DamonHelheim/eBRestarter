using eBRestarter.Core.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Windows.ApplicationModel;

namespace eBRestarter.Infrastructure.Services
{
    /// <summary>
    /// Implementiert den IAppInfoService für Windows.
    /// </summary>
    public class WindowsAppVersionInfoService : IAppVersionInfoService
    {
        public string GetAppVersion()
        {
            try
            {
                // 1. Versuch: WinUI 3 / UWP Paket-Version (MSIX)
                // Wenn die App als MSIX verpackt ist, steht die Version im Manifest.
                PackageVersion version = Package.Current.Id.Version;
                return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            }
            catch
            {
                // 2. Fallback: Unpackaged App (Klassische .NET Assembly)
                // Falls Package.Current fehlschlägt (z.B. beim Debuggen ohne MSIX)
                var assemblyVersion = Assembly.GetEntryAssembly()?.GetName().Version;

                if (assemblyVersion != null)
                {
                    return $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}.{assemblyVersion.Revision}";
                }

                return "1.0.0.0"; // Letzter Fallback
            }
        }
    }
}
