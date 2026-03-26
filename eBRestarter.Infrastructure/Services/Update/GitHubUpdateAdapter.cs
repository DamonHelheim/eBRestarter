using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Application.Interfaces.RestClient;
using eBRestarter.Core.Application.Interfaces.Update;
using eBRestarter.Core.Domain.Models;
using eBRestarter.Core.Domain.Models.Records;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace eBRestarter.Infrastructure.Services.Update;

public class GitHubUpdateAdapter(
    IRestClientService restClient,
    IWindowsProcessControlService processService,
    ILogger<GitHubUpdateAdapter> logger) : IUpdateService
{
    private readonly IRestClientService _restClient = restClient;
    private readonly IWindowsProcessControlService _processService = processService;
    private readonly ILogger<GitHubUpdateAdapter> _logger = logger;

    // Anpassen an dein Repository!
    private const string RepoOwner = "DamonHelheim";
    private const string RepoName = "eBRestarter";

    // GitHub API URL für das allerneueste Release
    private const string GitHubApiUrl = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest";

    public async Task<UpdateInfo> CheckForUpdateAsync()
    {
        var currentVersion = Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(1, 0, 0);

        var request = new ApiRequest
        {
            Url = GitHubApiUrl,
            Method = Core.Domain.Enums.HttpMethod.GET
        };

        var response = await _restClient.ExecuteGetAsync(request);

        if (!response.IsSuccess || string.IsNullOrEmpty(response.Content))
        {
            _logger.LogWarning("Update-Check fehlgeschlagen: {Status}", response.StatusCode);

            return new UpdateInfo { IsUpdateAvailable = false, CurrentVersion = currentVersion.ToString() };
        }

        try
        {
            using var doc = JsonDocument.Parse(response.Content);
            var root = doc.RootElement;

            // 1. Version aus dem Tag lesen (z.B. "v1.2.0" -> "1.2.0")
            string tagName = root.GetProperty("tag_name").GetString() ?? "0.0.0";
            string cleanVersion = tagName.TrimStart('v'); // "v" entfernen falls vorhanden

            if (!Version.TryParse(cleanVersion, out Version? latestVersion))
            {
                return new UpdateInfo { IsUpdateAvailable = false, CurrentVersion = currentVersion.ToString() };
            }

            // 2. Download URL für das Asset finden (z.B. Installer.msi oder Setup.exe)
            string downloadUrl = string.Empty;
            if (root.TryGetProperty("assets", out JsonElement assets) && assets.ValueKind == JsonValueKind.Array)
            {
                foreach (var asset in assets.EnumerateArray())
                {
                    string name = asset.GetProperty("name").GetString() ?? "";
                    // Sucht nach .exe oder .msi
                    if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                        name.EndsWith(".msi", StringComparison.OrdinalIgnoreCase))
                    {
                        downloadUrl = asset.GetProperty("browser_download_url").GetString() ?? "";
                        break;
                    }
                }
            }

            bool updateAvailable = latestVersion > currentVersion;

            return new UpdateInfo
            {
                IsUpdateAvailable = updateAvailable,
                CurrentVersion = currentVersion.ToString(),
                LatestVersion = cleanVersion,
                DownloadUrl = downloadUrl,
                Changelog = root.GetProperty("body").GetString() ?? "",
                PublishedAt = root.GetProperty("published_at").GetDateTime()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Parsen der GitHub Response.");
            return new UpdateInfo { IsUpdateAvailable = false };
        }
    }

    public async Task DownloadAndInstallAsync(UpdateInfo updateInfo)
    {
        if (string.IsNullOrEmpty(updateInfo.DownloadUrl))
        {
            _logger.LogError("Keine Download-URL gefunden.");
            return;
        }

        try
        {
            // Pfad zum Temp Ordner
            string tempFile = Path.Combine(Path.GetTempPath(), $"eBRestarter_Update_{updateInfo.LatestVersion}.exe");

            // WICHTIG: Hier brauchen wir einen Download, der Binary Data speichert.
            // Dein aktueller RestClient gibt Strings zurück. Für Dateien nutzen wir besser HttpClient direkt
            // oder erweitern den RestClient. Hier der Einfachheit halber HttpClient:

            using (var httpClient = new HttpClient())
            {
                // GitHub erfordert User-Agent Header
                httpClient.DefaultRequestHeaders.Add("User-Agent", "eBRestarter-App");

                var data = await httpClient.GetByteArrayAsync(updateInfo.DownloadUrl);
                await File.WriteAllBytesAsync(tempFile, data);
            }

            _logger.LogInformation("Update heruntergeladen nach: {Path}", tempFile);

            // Installer starten
            // Wir nutzen deinen ProcessService, aber wir müssen sicherstellen, dass wir Argumente übergeben können
            // oder wir nutzen Process.Start direkt hier, da es ein sehr spezifischer Infrastruktur-Case ist.

            var startInfo = new ProcessStartInfo
            {
                FileName = tempFile,
                UseShellExecute = true,
                // Argumente für "Silent Install" falls gewünscht (hängt vom Installer ab)
                // Arguments = "/passive"
            };

            Process.Start(startInfo);

            // App beenden, damit der Installer Dateien überschreiben kann
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Download/Installieren des Updates.");
            throw; // Werfe Fehler, damit ViewModel Bescheid weiß
        }
    }
}
