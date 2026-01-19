using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Browser; // IBrowserFactory, IBrowser
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.OperatingSystem; // IWindowsProcessControlService
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Domain.Enums; // BrowserType Enum
using eBRestarter.Core.Domain.Models.Records;
using eBRestarter.Core.Domain.Models.Records.Config;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using eBRestarter.Infrastructure.Browsers.Abstract;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{

    public partial class ViewModelDeleteBrowserContent : ObservableObject
    {
        #region Fields (Private Felder OHNE [ObservableProperty])

        private readonly IBrowserFactory _browserFactory;
        private readonly IEVisitorConfigService _iEVisitorConfigService;
        private readonly IDialogService _dialogService;
        private readonly IFileDeletionService _fileDeletionService;
        private readonly IWindowsProcessControlService _processService; // Dein existierender Service
        private BrowserPaths? _browserPaths; // Record mit Cache/Cookies Pfaden (jetzt Listen)
        private CancellationTokenSource? _cts;
        private IBrowser? _currentBrowser;
        private string _processName = ""; // Prozessname für Kill (z.B. "chrome")

        #endregion

        #region Observable Properties (Felder MIT [ObservableProperty])

        [ObservableProperty] public partial string BrowserIconPath { get; set; } = string.Empty; // WinUI Assets Pfad
        [ObservableProperty] public partial string BrowserName { get; set; } = "Lade...";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StartCleaningCommand))]
        [NotifyCanExecuteChangedFor(nameof(CancelCleaningCommand))]
        public partial bool IsBusy { get; set; } = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StartCleaningCommand))]
        public partial bool IsDeleteCookiesChecked { get; set; } = true;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StartCleaningCommand))]
        public partial bool IsDeleteInternetCacheChecked { get; set; } = true;

        [ObservableProperty] public partial bool IsProcessConflict { get; set; } = false;
        [ObservableProperty] public partial double ProgressMaximum { get; set; } = 100;
        [ObservableProperty] public partial string ProgressText { get; set; } = "0 %";
        [ObservableProperty] public partial double ProgressValue { get; set; } = 0;
        [ObservableProperty] public partial string StatusText { get; set; } = "Bereit.";

        #endregion

        #region Properties (Explizite get; set; Eigenschaften)

        private bool CanCancel() => IsBusy;
        private bool CanClean() => !IsBusy && (IsDeleteCookiesChecked || IsDeleteInternetCacheChecked);

        #endregion

        #region Constructors

        public ViewModelDeleteBrowserContent(
            IBrowserFactory browserFactory,
            IWindowsProcessControlService processService,
            IFileDeletionService fileDeletionService,
            IEVisitorConfigService iEVisitorConfigService,
            IDialogService dialogService)
        {
            _browserFactory = browserFactory;
            _iEVisitorConfigService = iEVisitorConfigService;
            _processService = processService;
            _fileDeletionService = fileDeletionService;
            _dialogService = dialogService;

            // 1. Config laden
            AppConfig config = iEVisitorConfigService.LoadConfig();

            // 2. Den String aus "config.Browser.Selected" holen (z.B. "Chrome")
            string selectedBrowserString = config.Browser.Selected;

            // 3. String in Enum umwandeln und initialisieren
            if (Enum.TryParse(typeof(BrowserType), selectedBrowserString, true, out var result))
            {
                // Erfolgreich geparst -> Initialisieren
                var browserType = (BrowserType)result;
                Initialize(browserType);
            }
            else
            {
                // Fallback, falls in der Config Quatsch steht oder sie leer ist
                StatusText = $"Konfiguration fehlerhaft: Unbekannter Browser '{selectedBrowserString}'.";
                // Optional: Standard laden oder UI deaktivieren
                // Initialize(BrowserType.Chrome); 
            }
        }

        #endregion

        #region Commands (Methoden MIT [RelayCommand])

        [RelayCommand(CanExecute = nameof(CanCancel))]
        private void CancelCleaning()
        {
            _cts?.Cancel();
            StatusText = "Breche ab...";
        }

        [RelayCommand(CanExecute = nameof(CanClean))]
        private async Task StartCleaning()
        {
            if (IsBusy || _currentBrowser == null || _browserPaths == null) return;

            // 1. Prozess Prüfung
            if (_processService.IsProcessAlive(_processName))
            {
                // STATT DIALOG: Wir schalten den Konflikt-Modus an
                IsProcessConflict = true;
                StatusText = "Browser läuft noch. Bitte Aktion wählen.";
                return; // Wir brechen hier ab und warten auf die User-Eingabe (siehe Commands unten)
            }

            // Wenn kein Prozess läuft, direkt weitermachen
            await ExecuteCleaningLogic();
        }

        [RelayCommand]
        private async Task ForceCloseAndContinue()
        {
            IsProcessConflict = false; // Warnung ausblenden

            // Versuchen zu schließen
            _processService.CloseApplication(_processName);
            StatusText = "Beende Browser...";
            await Task.Delay(1000); // Kurz warten

            // Erneute Prüfung
            if (_processService.IsProcessAlive(_processName))
            {
                StatusText = "Konnte Browser nicht beenden. Bitte manuell schließen.";
                // Optional: IsProcessConflict wieder auf true setzen, wenn man hartnäckig sein will
                return;
            }

            // Wenn erfolgreich geschlossen, eigentliche Logik ausführen
            await ExecuteCleaningLogic();
        }

        [RelayCommand]
        private void CancelConflict()
        {
            IsProcessConflict = false; // Warnung ausblenden
            StatusText = "Abbruch durch Benutzer.";
        }

        // --- DIE EIGENTLICHE LÖSCH-LOGIK (Ausgelagert) ---
        private async Task ExecuteCleaningLogic()
        {
            // 2. Pfade sammeln (Dein bestehender Code)
            var directoriesToDelete = new List<string>();

            if (IsDeleteInternetCacheChecked && _browserPaths.CacheDirs != null && _browserPaths.CacheDirs.Count > 0)
                directoriesToDelete.AddRange(_browserPaths.CacheDirs);

            if (IsDeleteCookiesChecked && _browserPaths.CookiesDirs != null && _browserPaths.CookiesDirs.Count > 0)
                directoriesToDelete.AddRange(_browserPaths.CookiesDirs);

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

                // ... Dein bestehender Lösch-Code (CountFilesAsync, DeleteFilesAsync etc.) ...
                int totalFiles = await _fileDeletionService.CountFilesAsync(directoriesToDelete);
                ProgressMaximum = totalFiles > 0 ? totalFiles : 1;
                ProgressValue = 0;

                var statusProgress = new Progress<string>(msg => StatusText = msg);
                var valueProgress = new Progress<int>(val =>
                {
                    ProgressValue = val;
                    if (totalFiles > 0) ProgressText = $"{(val * 100 / totalFiles)} %";
                });

                await _fileDeletionService.DeleteFilesAsync(directoriesToDelete, statusProgress, valueProgress, _cts.Token);
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

        //[RelayCommand(CanExecute = nameof(CanClean))]
        //private async Task StartCleaning()
        //{
        //    if (IsBusy || _currentBrowser == null || _browserPaths == null) return;

        //    // 1. Prozess Prüfung mit deinem Service
        //    if (_processService.IsProcessAlive(_processName))
        //    {
        //        var result = await _dialogService.ShowYesNoDialogAsync(
        //            "Browser schließen?",
        //            $"Der Browser '{BrowserName}' läuft noch. Er muss beendet werden, um zu bereinigen.");

        //        if (result)
        //        {
        //            _processService.CloseApplication(_processName);
        //            // Kurz warten, damit Prozess wirklich weg ist
        //            await Task.Delay(1000);

        //            // Double Check
        //            if (_processService.IsProcessAlive(_processName))
        //            {
        //                StatusText = "Konnte Browser nicht beenden.";
        //                return;
        //            }
        //        }
        //        else
        //        {
        //            StatusText = "Abbruch durch Benutzer.";
        //            return;
        //        }
        //    }

        //    // 2. Pfade sammeln (ANGEPASST für Multi-Profil Support)
        //    var directoriesToDelete = new List<string>();

        //    // CACHE: Jetzt prüfen wir die Liste "CacheDirs" und fügen alle hinzu
        //    if (IsDeleteInternetCacheChecked && _browserPaths.CacheDirs != null && _browserPaths.CacheDirs.Count > 0)
        //    {
        //        directoriesToDelete.AddRange(_browserPaths.CacheDirs);
        //    }

        //    // COOKIES: Jetzt prüfen wir die Liste "CookiesDirs" und fügen alle hinzu
        //    if (IsDeleteCookiesChecked && _browserPaths.CookiesDirs != null && _browserPaths.CookiesDirs.Count > 0)
        //    {
        //        directoriesToDelete.AddRange(_browserPaths.CookiesDirs);
        //    }

        //    // Extensions (falls gewünscht) - Analog für ExtensionsDirs falls benötigt
        //    // if (_browserPaths.ExtensionsDirs != null) directoriesToDelete.AddRange(_browserPaths.ExtensionsDirs);

        //    if (directoriesToDelete.Count == 0)
        //    {
        //        StatusText = "Keine gültigen Pfade gefunden.";
        //        return;
        //    }

        //    IsBusy = true;
        //    _cts = new CancellationTokenSource();

        //    try
        //    {
        //        StatusText = "Analysiere Dateien...";

        //        // Zählen
        //        int totalFiles = await _fileDeletionService.CountFilesAsync(directoriesToDelete);
        //        ProgressMaximum = totalFiles > 0 ? totalFiles : 1;
        //        ProgressValue = 0;

        //        var statusProgress = new Progress<string>(msg => StatusText = msg);
        //        var valueProgress = new Progress<int>(val =>
        //        {
        //            ProgressValue = val;
        //            if (totalFiles > 0)
        //                ProgressText = $"{(val * 100 / totalFiles)} %";
        //        });

        //        // Löschen
        //        await _fileDeletionService.DeleteFilesAsync(directoriesToDelete, statusProgress, valueProgress, _cts.Token);

        //        StatusText = "Bereinigung abgeschlossen.";
        //    }
        //    catch (Exception ex)
        //    {
        //        StatusText = $"Fehler: {ex.Message}";
        //    }
        //    finally
        //    {
        //        IsBusy = false;
        //        _cts = null;
        //    }
        //}

        #endregion

        #region Methods (Restliche Methoden)

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
                BrowserIconPath = _currentBrowser.IconPath;

                // 3. Pfade laden (über die neue GetPaths Methode im Browser)
                _browserPaths = _currentBrowser.GetPaths();

                // 4. Prozessnamen ermitteln (Den brauchen wir für IsProcessAlive)
                _processName = GetProcessNameByType(selectedBrowserType);
            }
            catch (Exception ex)
            {
                StatusText = $"Fehler beim Laden: {ex.Message}";
            }
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

        #endregion
    }

    //public partial class ViewModelDeleteBrowserContent : ObservableObject
    //{
    //    #region Fields (Private Felder OHNE [ObservableProperty])

    //    private readonly IBrowserFactory _browserFactory;
    //    private readonly IEVisitorConfigService _iEVisitorConfigService;
    //    private readonly IDialogService _dialogService;
    //    private readonly IFileDeletionService _fileDeletionService;
    //    private readonly IWindowsProcessControlService _processService; // Dein existierender Service
    //    private BrowserPaths? _browserPaths; // Record mit Cache/Cookies Pfaden
    //    private CancellationTokenSource? _cts;
    //    private IBrowser? _currentBrowser;
    //    private string _processName = ""; // Prozessname für Kill (z.B. "chrome")

    //    #endregion

    //    #region Observable Properties (Felder MIT [ObservableProperty])

    //    [ObservableProperty] public partial string BrowserIconPath { get; set; } = string.Empty; // WinUI Assets Pfad
    //    [ObservableProperty] public partial string BrowserName { get; set; } = "Lade...";

    //    [ObservableProperty]
    //    [NotifyCanExecuteChangedFor(nameof(StartCleaningCommand))]
    //    [NotifyCanExecuteChangedFor(nameof(CancelCleaningCommand))]
    //    public partial bool IsBusy { get; set; } = false;

    //    [ObservableProperty]
    //    [NotifyCanExecuteChangedFor(nameof(StartCleaningCommand))]
    //    public partial bool IsDeleteCookiesChecked { get; set; } = true;

    //    [ObservableProperty]
    //    [NotifyCanExecuteChangedFor(nameof(StartCleaningCommand))]
    //    public partial bool IsDeleteInternetCacheChecked { get; set; } = true;

    //    [ObservableProperty] public partial double ProgressMaximum { get; set; } = 100;
    //    [ObservableProperty] public partial string ProgressText { get; set; } = "0 %";
    //    [ObservableProperty] public partial double ProgressValue { get; set; } = 0;
    //    [ObservableProperty] public partial string StatusText { get; set; } = "Bereit.";

    //    #endregion

    //    #region Properties (Explizite get; set; Eigenschaften)

    //    private bool CanCancel() => IsBusy;
    //    private bool CanClean() => !IsBusy && (IsDeleteCookiesChecked || IsDeleteInternetCacheChecked);

    //    #endregion

    //    #region Constructors

    //    public ViewModelDeleteBrowserContent(
    //        IBrowserFactory browserFactory,
    //        IWindowsProcessControlService processService,
    //        IFileDeletionService fileDeletionService,
    //        IEVisitorConfigService iEVisitorConfigService,
    //        IDialogService dialogService)
    //    {
    //        _browserFactory = browserFactory;
    //        _iEVisitorConfigService = iEVisitorConfigService;
    //        _processService = processService;
    //        _fileDeletionService = fileDeletionService;
    //        _dialogService = dialogService;

    //        // 1. Config laden
    //        AppConfig config = iEVisitorConfigService.LoadConfig();

    //        // 2. Den String aus "config.Browser.Selected" holen (z.B. "Chrome")
    //        string selectedBrowserString = config.Browser.Selected;

    //        // 3. String in Enum umwandeln und initialisieren
    //        if (Enum.TryParse(typeof(BrowserType), selectedBrowserString, true, out var result))
    //        {
    //            // Erfolgreich geparst -> Initialisieren
    //            var browserType = (BrowserType)result;
    //            Initialize(browserType);
    //        }
    //        else
    //        {
    //            // Fallback, falls in der Config Quatsch steht oder sie leer ist
    //            StatusText = $"Konfiguration fehlerhaft: Unbekannter Browser '{selectedBrowserString}'.";
    //            // Optional: Standard laden oder UI deaktivieren
    //            // Initialize(BrowserType.Chrome); 
    //        }
    //    }

    //    #endregion

    //    #region Commands (Methoden MIT [RelayCommand])

    //    [RelayCommand(CanExecute = nameof(CanCancel))]
    //    private void CancelCleaning()
    //    {
    //        _cts?.Cancel();
    //        StatusText = "Breche ab...";
    //    }

    //    [RelayCommand(CanExecute = nameof(CanClean))]
    //    private async Task StartCleaning()
    //    {
    //        if (IsBusy || _currentBrowser == null || _browserPaths == null) return;

    //        // 1. Prozess Prüfung mit deinem Service
    //        if (_processService.IsProcessAlive(_processName))
    //        {
    //            var result = await _dialogService.ShowYesNoDialogAsync(
    //                "Browser schließen?",
    //                $"Der Browser '{BrowserName}' läuft noch. Er muss beendet werden, um zu bereinigen.");

    //            if (result)
    //            {
    //                _processService.CloseApplication(_processName);
    //                // Kurz warten, damit Prozess wirklich weg ist
    //                await Task.Delay(1000);

    //                // Double Check
    //                if (_processService.IsProcessAlive(_processName))
    //                {
    //                    StatusText = "Konnte Browser nicht beenden.";
    //                    return;
    //                }
    //            }
    //            else
    //            {
    //                StatusText = "Abbruch durch Benutzer.";
    //                return;
    //            }
    //        }

    //        // 2. Pfade sammeln
    //        var directoriesToDelete = new List<string>();

    //        if (IsDeleteInternetCacheChecked && !string.IsNullOrEmpty(_browserPaths.CacheDir))
    //            directoriesToDelete.Add(_browserPaths.CacheDir);

    //        if (IsDeleteCookiesChecked && !string.IsNullOrEmpty(_browserPaths.CookiesDir))
    //            directoriesToDelete.Add(_browserPaths.CookiesDir);

    //        // Extensions (falls gewünscht, war im alten Code nicht explizit an/aus schaltbar, aber vorhanden)
    //        // if (!string.IsNullOrEmpty(_browserPaths.ExtensionsDir)) directoriesToDelete.Add(_browserPaths.ExtensionsDir);

    //        if (directoriesToDelete.Count == 0)
    //        {
    //            StatusText = "Keine gültigen Pfade gefunden.";
    //            return;
    //        }

    //        IsBusy = true;
    //        _cts = new CancellationTokenSource();

    //        try
    //        {
    //            StatusText = "Analysiere Dateien...";

    //            // Zählen
    //            int totalFiles = await _fileDeletionService.CountFilesAsync(directoriesToDelete);
    //            ProgressMaximum = totalFiles > 0 ? totalFiles : 1;
    //            ProgressValue = 0;

    //            var statusProgress = new Progress<string>(msg => StatusText = msg);
    //            var valueProgress = new Progress<int>(val =>
    //            {
    //                ProgressValue = val;
    //                if (totalFiles > 0)
    //                    ProgressText = $"{(val * 100 / totalFiles)} %";
    //            });

    //            // Löschen
    //            await _fileDeletionService.DeleteFilesAsync(directoriesToDelete, statusProgress, valueProgress, _cts.Token);

    //            // Firefox Spezialfall (Einzelne Datei für Cookies.sqlite? Dein alter Code hatte sowas)
    //            // Wenn _currentBrowser Firefox ist, müssen wir evtl. speziell handeln, 
    //            // falls GetPaths().CookiesDir nicht reicht (Firefox nutzt cookies.sqlite Datei, Chrome nutzt Ordner).
    //            // Der FileDeletionService sollte erweitert werden, um auch File-Paths statt nur Dir-Paths zu akzeptieren.

    //            StatusText = "Bereinigung abgeschlossen.";
    //        }
    //        catch (Exception ex)
    //        {
    //            StatusText = $"Fehler: {ex.Message}";
    //        }
    //        finally
    //        {
    //            IsBusy = false;
    //            _cts = null;
    //        }
    //    }

    //    #endregion

    //    #region Methods (Restliche Methoden)

    //    // Diese Methode muss aufgerufen werden (z.B. vom Parent ViewModel), 
    //    // und zwar mit dem BrowserType aus der Config.
    //    public void Initialize(BrowserType selectedBrowserType)
    //    {
    //        try
    //        {
    //            // 1. Browser Instanz über Factory holen
    //            _currentBrowser = _browserFactory.Create(selectedBrowserType);

    //            // 2. UI Daten setzen
    //            BrowserName = _currentBrowser.DisplayName;
    //            // Achtung: Der IconPath aus IBrowser ist oft "/Resources...", 
    //            // für WinUI müssen wir das evtl. auf "ms-appx:///Assets/..." mappen oder im Browser fixen.
    //            BrowserIconPath = _currentBrowser.IconPath;

    //            // 3. Pfade laden (über die neue GetPaths Methode im Browser)
    //            _browserPaths = _currentBrowser.GetPaths();

    //            // 4. Prozessnamen ermitteln (Den brauchen wir für IsProcessAlive)
    //            // Leider ist ProcessName in IBrowser nicht public. 
    //            // Lösung: Entweder IBrowser erweitern um "ProcessName" Property (Empfohlen!)
    //            // Workaround hier: Hardcoding basierend auf Type
    //            _processName = GetProcessNameByType(selectedBrowserType);
    //        }
    //        catch (Exception ex)
    //        {
    //            StatusText = $"Fehler beim Laden: {ex.Message}";
    //        }
    //    }

    //    private string GetProcessNameByType(BrowserType type)
    //    {
    //        // Am besten in IBrowser als Property aufnehmen!
    //        return type switch
    //        {
    //            BrowserType.Chrome => "chrome",
    //            BrowserType.Firefox => "firefox",
    //            BrowserType.Edge => "msedge", // Wichtig: msedge!
    //            BrowserType.Brave => "brave",
    //            _ => ""
    //        };
    //    }

    //    #endregion
    //}
}
