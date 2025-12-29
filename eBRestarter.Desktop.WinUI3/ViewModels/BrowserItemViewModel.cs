using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBRestarter.Core.Domain.Models;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public partial class BrowserItemViewModel : ObservableObject
    {
        private readonly BrowserInfo _browserInfo;

        public BrowserItemViewModel(BrowserInfo info)
        {
            _browserInfo = info;
        }

        public string Name => _browserInfo.Name;
        public string Icon => _browserInfo.IconPath;

        // UI-Logik: Textformatierung
        public string VersionText => _browserInfo.IsInstalled ? $"Version: {_browserInfo.Version}" : "Nicht installiert";

        // UI-Logik: Farben & Icons über Properties (besser als Converter für einfache Logik)
        public SolidColorBrush StatusColor => _browserInfo.IsInstalled ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
        public string StatusSymbol => _browserInfo.IsInstalled ? "✓" : "✘";

        // Sichtbarkeiten (Boolesche Werte für x:Bind oder Converter)
        public bool IsDownloadButtonVisible => !_browserInfo.IsInstalled;
        public bool IsChooseButtonVisible => _browserInfo.IsInstalled;

        // Commands
        [RelayCommand]
        private void Download() { /* ... */ }

        [RelayCommand]
        private void Choose() { /* ... */ }
    }
}
