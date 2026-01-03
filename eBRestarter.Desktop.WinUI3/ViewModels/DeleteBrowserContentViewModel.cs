using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Browser; // IBrowserFactory, IBrowser
using eBRestarter.Core.Application.Interfaces.OperatingSystem; // IWindowsProcessControlService
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Domain.Enums; // BrowserType Enum
using eBRestarter.Core.Domain.Models.Records;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using eBRestarter.Infrastructure.Browsers.Abstract;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public partial class DeleteBrowserContentViewModel : ObservableObject
    {
        private readonly IBrowserFactory _browserFactory;
        private readonly IWindowsProcessControlService _processService; // Dein existierender Service
        private readonly IFileDeletionService _fileDeletionService;
        private readonly IDialogService _dialogService;

        // Aktueller Browser (Instanz)
        private IBrowser? _currentBrowser;
        private BrowserPaths? _browserPaths; // Record mit Cache/Cookies Pfaden
        private string _processName = ""; // Prozessname für Kill (z.B. "chrome")

        private CancellationTokenSource? _cts;

        // --- UI Properties ---
        [ObservableProperty] public partial string BrowserName { get; set; } = "Lade...";
        [ObservableProperty] public partial string BrowserIconPath { get; set; }  = string.Empty; // WinUI Assets Pfad

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StartCleaningCommand))]
        private partial bool IsDeleteCookiesChecked { get; set; } = true;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StartCleaningCommand))]
        private partial bool IsDeleteInternetCacheChecked { get; set; } = true;

        [ObservableProperty] public partial double ProgressValue { get; set; } = 0;
        [ObservableProperty] public partial double ProgressMaximum { get; set; } = 100;
        [ObservableProperty] public partial string ProgressText { get; set; } = "0 %";
        [ObservableProperty] public partial string StatusText { get; set; } = "Bereit.";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StartCleaningCommand))]
        [NotifyCanExecuteChangedFor(nameof(CancelCleaningCommand))]
        private partial bool IsBusy { get; set; } = false;

        public DeleteBrowserContentViewModel(
            IBrowserFactory browserFactory,
            IWindowsProcessControlService processService,
            IFileDeletionService fileDeletionService,
            IDialogService dialogService)
        {
            _browserFactory = browserFactory;
            _processService = processService;
            _fileDeletionService = fileDeletionService;
            _dialogService = dialogService;
        }

        // Diese Methode muss aufgerufen werden (z.B. vom Parent ViewModel), 
        // und zwar mit dem BrowserType aus der Config.
        public void Initialize(BrowserType selectedBrowserType)
        {
            try
            {
                // 1. Browser Instanz über Factory holen
                _currentBrowser = _browserFactory.Create(selectedBrowserType);

                // 2. UI Daten setzen
                BrowserName = _currentBrowser.DisplayName;
                // Achtung: Der IconPath aus IBrowser ist oft "/Resources...", 
                // für WinUI müssen wir das evtl. auf "ms-appx:///Assets/..." mappen oder im Browser fixen.
                BrowserIconPath = FixIconPathForWinUI(_currentBrowser.IconPath);

                // 3. Pfade laden (über die neue GetPaths Methode im Browser)
                _browserPaths = _currentBrowser.GetPaths();

                // 4. Prozessnamen ermitteln (Den brauchen wir für IsProcessAlive)
                // Leider ist ProcessName in IBrowser nicht public. 
                // Lösung: Entweder IBrowser erweitern um "ProcessName" Property (Empfohlen!)
                // Workaround hier: Hardcoding basierend auf Type
                _processName = GetProcessNameByType(selectedBrowserType);
            }
            catch (Exception ex)
            {
                StatusText = $"Fehler beim Laden: {ex.Message}";
            }
        }

        [RelayCommand(CanExecute = nameof(CanClean))]
        private async Task StartCleaning()
        {
            if (IsBusy || _currentBrowser == null || _browserPaths == null) return;

            // 1. Prozess Prüfung mit deinem Service
            if (_processService.IsProcessAlive(_processName))
            {
                var result = await _dialogService.ShowYesNoDialogAsync(
                    "Browser schließen?",
                    $"Der Browser '{BrowserName}' läuft noch. Er muss beendet werden, um zu bereinigen.");

                if (result)
                {
                    _processService.CloseApplication(_processName);
                    // Kurz warten, damit Prozess wirklich weg ist
                    await Task.Delay(1000);

                    // Double Check
                    if (_processService.IsProcessAlive(_processName))
                    {
                        StatusText = "Konnte Browser nicht beenden.";
                        return;
                    }
                }
                else
                {
                    StatusText = "Abbruch durch Benutzer.";
                    return;
                }
            }

            // 2. Pfade sammeln
            var directoriesToDelete = new List<string>();

            if (IsDeleteInternetCacheChecked && !string.IsNullOrEmpty(_browserPaths.CacheDir))
                directoriesToDelete.Add(_browserPaths.CacheDir);

            if (IsDeleteCookiesChecked && !string.IsNullOrEmpty(_browserPaths.CookiesDir))
                directoriesToDelete.Add(_browserPaths.CookiesDir);

            // Extensions (falls gewünscht, war im alten Code nicht explizit an/aus schaltbar, aber vorhanden)
            // if (!string.IsNullOrEmpty(_browserPaths.ExtensionsDir)) directoriesToDelete.Add(_browserPaths.ExtensionsDir);

            if (directoriesToDelete.Count == 0)
            {
                StatusText = "Keine gültigen Pfade gefunden.";
                return;
            }

            IsBusy = true;
            _cts = new CancellationTokenSource();

            try
            {
                StatusText = "Analysiere Dateien...";

                // Zählen
                int totalFiles = await _fileDeletionService.CountFilesAsync(directoriesToDelete);
                ProgressMaximum = totalFiles > 0 ? totalFiles : 1;
                ProgressValue = 0;

                var statusProgress = new Progress<string>(msg => StatusText = msg);
                var valueProgress = new Progress<int>(val =>
                {
                    ProgressValue = val;
                    if (totalFiles > 0)
                        ProgressText = $"{(val * 100 / totalFiles)} %";
                });

                // Löschen
                await _fileDeletionService.DeleteFilesAsync(directoriesToDelete, statusProgress, valueProgress, _cts.Token);

                // Firefox Spezialfall (Einzelne Datei für Cookies.sqlite? Dein alter Code hatte sowas)
                // Wenn _currentBrowser Firefox ist, müssen wir evtl. speziell handeln, 
                // falls GetPaths().CookiesDir nicht reicht (Firefox nutzt cookies.sqlite Datei, Chrome nutzt Ordner).
                // Der FileDeletionService sollte erweitert werden, um auch File-Paths statt nur Dir-Paths zu akzeptieren.

                StatusText = "Bereinigung abgeschlossen.";
            }
            catch (Exception ex)
            {
                StatusText = $"Fehler: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
                _cts = null;
            }
        }

        [RelayCommand(CanExecute = nameof(CanCancel))]
        private void CancelCleaning()
        {
            _cts?.Cancel();
            StatusText = "Breche ab...";
        }

        private bool CanClean() => !IsBusy && (IsDeleteCookiesChecked || IsDeleteInternetCacheChecked);
        private bool CanCancel() => IsBusy;

        // Hilfsmethode für Pfadkorrektur (WPF -> WinUI)
        private string FixIconPathForWinUI(string path)
        {
            // Dein WPF Pfad war "/Resources/Visuals/..."
            // WinUI will "ms-appx:///Assets/..."
            // Mappe das hier oder ändere es direkt in den Browser-Klassen (Besser!)
            return path.Replace("/Resources/Visuals", "ms-appx:///Assets/Visuals");
        }

        private string GetProcessNameByType(BrowserType type)
        {
            // Am besten in IBrowser als Property aufnehmen!
            return type switch
            {
                BrowserType.Chrome => "chrome",
                BrowserType.Firefox => "firefox",
                BrowserType.Edge => "msedge", // Wichtig: msedge!
                BrowserType.Brave => "brave",
                _ => ""
            };
        }
    }
}
