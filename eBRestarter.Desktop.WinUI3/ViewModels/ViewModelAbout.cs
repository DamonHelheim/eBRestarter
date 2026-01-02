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
        private readonly IAppInfoService _appInfoService;

        [ObservableProperty]
        public partial string AppVersion { get; set; } = "Lade...";

        // Statt SP_Icon_Container.Children.Add nutzen wir DataBinding an eine Liste
        public ObservableCollection<IconCredit> IconCredits { get; } = [];

        public ViewModelAbout(IAppInfoService appInfoService)
        {
            _appInfoService = appInfoService;
            LoadData();
        }

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
    }
}
