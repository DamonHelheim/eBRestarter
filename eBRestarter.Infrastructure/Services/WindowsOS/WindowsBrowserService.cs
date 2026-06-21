using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models;
using System.Diagnostics;

namespace eBRestarter.Infrastructure.Services.WindowsOS;

public class WindowsBrowserService(IBrowserFactory browserFactory) : IBrowserService
{
    private readonly IBrowserFactory _browserFactory = browserFactory ?? throw new ArgumentNullException(nameof(browserFactory));

    public async Task<IEnumerable<BrowserInfo>> FindInstalledBrowsersAsync()
    {
        var browsers = new List<BrowserInfo>();

        // Wir iterieren dynamisch Ã¼ber alle Werte im Enum BrowserType.
        // Das macht den Code zukunftssicher: Neuer Browser im Enum + Factory = automatisch hier drin.
        foreach (BrowserType type in Enum.GetValues<BrowserType>())
        {
            try
            {
                // 1. Factory nutzen, um die Logik-Klasse fÃ¼r diesen Browser zu bekommen
                // (z.B. ChromeBrowserAdapter, FirefoxBrowserAdapter...)
                IBrowser browserLogic = _browserFactory.Create(type);

                // 2. Daten abfragen
                // Deine BrowserBaseAdapter Implementierung kÃ¼mmert sich um die Details (Registry, Pfade)
                bool isInstalled = browserLogic.IsInstalled;
                string version = isInstalled ? browserLogic.BrowserVersion : "Nicht installiert";
                string displayName = browserLogic.DisplayName;
                string iconPath = browserLogic.IconPath;
                string downloadUrl = browserLogic.DownloadUrl;

                // 3. BrowserInfo Objekt erstellen (Mapping)
                // Hier mappen wir die Logik-Daten auf das einfache Daten-Objekt fÃ¼r die UI
                var browserInfo = new BrowserInfo
                {
                    Type = type,
                    Name = displayName, // Oder du fÃ¼gst eine 'Name' Property im IBrowser hinzu fÃ¼r "Google Chrome" statt "Chrome"
                    IsInstalled = isInstalled,
                    Version = version,
                    IconPath = iconPath,
                    DownloadUrl = downloadUrl,
                };

                browsers.Add(browserInfo);
            }
            catch (NotSupportedException)
            {
                // Falls ein Browser im Enum steht, aber noch nicht in der Factory implementiert ist.
                // Loggen wÃ¤re hier gut.
                continue;
            }
            catch (Exception ex)
            {
                // Fehler bei einem einzelnen Browser sollten nicht den ganzen Prozess stoppen
                // _logger.LogError(ex, $"Fehler beim Abrufen von Infos fÃ¼r {type}");
                Debug.WriteLine(ex);
            }
        }

        // Da Registry-Checks synchron sind, wrappen wir das Ergebnis in einen Task
        return await Task.FromResult(browsers);
    }
}


