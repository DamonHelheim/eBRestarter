using CommunityToolkit.Mvvm.ComponentModel;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Domain.Models.Records;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    /// <summary>
    /// View model for the About page. Displays the application version from
    /// <see cref="IAppInfoService"/> and a list of icon credits, both localized where applicable.
    /// </summary>
    public partial class ViewModelAbout : ObservableObject
    {
        // =========================================================
        // 1. FIELDS & INJECTED SERVICES (Backing-Felder und DI)
        // =========================================================
        #region FieldsAndInjectedServices

        private readonly IAppInfoService _appInfoService;
        private readonly ILocalizationService _localizationService;

        #endregion

        // =========================================================
        // 2. OBSERVABLE PROPERTIES (MVVM State)
        // =========================================================
        #region ObservableProperties

        [ObservableProperty] public partial string AppVersion { get; set; }

        #endregion

        // =========================================================
        // 3. PUBLIC PROPERTIES (Data & State)
        // =========================================================
        #region PublicProperties

        /// <summary>Collection of icon credit entries shown on the About page (e.g. author and license).</summary>
        public ObservableCollection<IconCredit> IconCredits { get; } = [];

        #endregion

        // =========================================================
        // 4. CONSTRUCTOR & FINALIZER (Ctor)
        // =========================================================
        #region ConstructorAndFinalizer

        /// <summary>
        /// Initializes the About VM with app-info and localization services, sets a loading placeholder
        /// for version, and loads version plus icon credits so the UI can bind immediately.
        /// </summary>
        /// <param name="appInfoService">Provides app version and icon credit data. Must not be null.</param>
        /// <param name="localizationService">Used for version prefix and other strings. Must not be null.</param>
        public ViewModelAbout(
            IAppInfoService appInfoService,
            ILocalizationService localizationService)
        {
            _appInfoService = appInfoService;
            _localizationService = localizationService;
            AppVersion = _localizationService.GetString("About_Loading");
            LoadData();
        }

        #endregion

        // =========================================================
        // 5. PRIVATE HELPER METHODS (Interne Hilfsmethoden)
        // =========================================================
        #region PrivateHelperMethods

        /// <summary>Fetches version from app info and icon credits, then updates AppVersion and IconCredits for binding.</summary>
        private void LoadData()
        {
            string prefix = _localizationService.GetString("About_VersionPrefix");
            AppVersion = $"{prefix} {_appInfoService.GetAppVersion()}";
            var credits = _appInfoService.GetIconCredits();
            IconCredits.Clear();
            foreach (var credit in credits)
                IconCredits.Add(credit);
        }

        #endregion
    }
}
