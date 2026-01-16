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

        #endregion

        #region Observable Properties (Felder MIT [ObservableProperty])

        [ObservableProperty]
        public partial string AppVersion { get; set; } = "Lade...";

        #endregion

        #region Properties (Explizite get; set; Eigenschaften)

        // Statt SP_Icon_Container.Children.Add nutzen wir DataBinding an eine Liste
        public ObservableCollection<IconCredit> IconCredits { get; } = [];

        #endregion

        #region Constructors

        public ViewModelAbout(IAppInfoService appInfoService)
        {
            _appInfoService = appInfoService;
            LoadData();
        }

        #endregion

        #region Methods (Restliche Methoden)

        private void LoadData()
        {
            AppVersion = $"Version: {_appInfoService.GetAppVersion()}";

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
