using CommunityToolkit.Mvvm.ComponentModel;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Domain.Models.Records;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public partial class ViewModelAbout : ObservableObject
    {
        #region Fields (Private Felder OHNE [ObservableProperty])

        private readonly IAppInfoService _appInfoService;
        private readonly ILocalizationService _localizationService; // <--- NEU: Service Feld

        #endregion

        #region Observable Properties (Felder MIT [ObservableProperty])

        // Initialwert entfernen wir hier, da wir ihn im Konstruktor setzen
        [ObservableProperty]
        public partial string AppVersion { get; set; }

        #endregion

        #region Properties (Explizite get; set; Eigenschaften)

        public ObservableCollection<IconCredit> IconCredits { get; } = [];

        #endregion

        #region Constructors

        public ViewModelAbout(
            IAppInfoService appInfoService,
            ILocalizationService localizationService) // <--- Injizieren
        {
            _appInfoService = appInfoService;
            _localizationService = localizationService; // <--- Zuweisen

            // Lokalisierter Startwert
            AppVersion = _localizationService.GetString("About_Loading"); // "Lade..."

            LoadData();
        }

        #endregion

        #region Methods (Restliche Methoden)

        private void LoadData()
        {
            string prefix = _localizationService.GetString("About_VersionPrefix"); // "Version:"
            AppVersion = $"{prefix} {_appInfoService.GetAppVersion()}";

            var credits = _appInfoService.GetIconCredits();
            IconCredits.Clear();
            foreach (var credit in credits)
            {
                IconCredits.Add(credit);
            }
        }

        #endregion
    }
}