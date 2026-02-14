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

        public ObservableCollection<IconCredit> IconCredits { get; } = [];

        #endregion

        // =========================================================
        // 4. CONSTRUCTOR & FINALIZER (Ctor)
        // =========================================================
        #region ConstructorAndFinalizer

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
