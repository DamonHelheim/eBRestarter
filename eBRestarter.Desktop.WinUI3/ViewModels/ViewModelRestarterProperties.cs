using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBRestarter.Application.Facade.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public partial class ViewModelRestarterProperties : ObservableObject
    {
        // Nur noch EINE Abhängigkeit
        private readonly IOperatingSystemFacade _os;

        // 1. Die Property an den Command binden
        // Das Attribut sagt: Wenn sich _username ändert, lade den Command neu!
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddeVVisitorUsernameCommand))]
        public partial string Username { get; set; } = string.Empty;

        [ObservableProperty]
        private partial string StandardBrowser { get; set; } = string.Empty;

        // Der Konstruktor ist extrem schlank
        public ViewModelRestarterProperties(IOperatingSystemFacade os)
        {
            _os = os;

            // Zugriff erfolgt nun hierarchisch: _os.SystemInfo...
            StandardBrowser = _os.SystemInfo.GetCurrentStandardBrowserName();
        }

        // 2. Der Command mit CanExecute-Prüfung
        [RelayCommand(CanExecute = nameof(CanAddUsername))]
        private void AddeVVisitorUsername()
        {
            // Deine Logik zum Speichern...
            System.Diagnostics.Debug.WriteLine($"User {Username} hinzugefügt.");
        }

        // 3. Die Logik: Wann darf der Button aktiv sein?
        private bool CanAddUsername()
        {
            // Button ist aktiv, wenn der String NICHT leer ist
            return !string.IsNullOrWhiteSpace(Username);
        }

        [RelayCommand]
        public void RegisterToEVisitor()
        {
            Debug.WriteLine("ViewModelRestarterProperties: RegisterToEVisitor");
        }
    }
}
