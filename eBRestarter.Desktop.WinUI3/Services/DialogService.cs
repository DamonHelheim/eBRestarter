using eBRestarter.Core.Domain.Models;
using eBRestarter.Core.Domain.Models.Records;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using eBRestarter.Desktop.WinUI3.Views.Dialogs;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.Services
{
    public class DialogService : IDialogService
    {
        // Du musst hier irgendwie an das XamlRoot kommen. 
        // Entweder übergebene UI-Elemente oder über App.MainWindow.Content.XamlRoot
        private XamlRoot _xamlRoot => App.MainWindoweBRestarter!.Content.XamlRoot;

        public Task ShowMessageAsync(string title, string message)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ShowYesNoDialogAsync(string title, string message)
        {
            var dialog = new ContentDialog
            {
                XamlRoot = _xamlRoot,
                Title = title,
                Content = message,
                PrimaryButtonText = "Ja",
                CloseButtonText = "Nein"
            };

            var result = await dialog.ShowAsync();

            return result == ContentDialogResult.Primary;
        }

        public async Task<AutoLogonDialogResult?> ShowAutoLogonDialogAsync(string defaultUser = null, string defaultDomain = null)
        {
            var dialog = new AutoLogonDialog
            {
                XamlRoot = _xamlRoot
            };

            // --- HIER IST DIE ÄNDERUNG ---
            // Wir übergeben die Daten an den Dialog, damit die Textboxen gefüllt werden
            dialog.SetDefaults(defaultUser, defaultDomain);
            // -----------------------------

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                return AutoLogonDialogResult.Save(dialog.GetCredentials());
            }
            else if (result == ContentDialogResult.Secondary)
            {
                return AutoLogonDialogResult.Deactivate();
            }

            return null;
        }

        public async Task ShowAboutDialogAsync()
        {
            // 1. Instanz erstellen
            var dialog = new AboutDialog
            {
                // 2. XamlRoot setzen (sonst stürzt WinUI 3 ab)
                XamlRoot = _xamlRoot
            };

            // 3. Anzeigen und warten, bis der Nutzer ihn schließt
            await dialog.ShowAsync();
        }

        public async Task ShowBrowserDeleteContentDialogAsync()
        {
            // 1. Instanz erstellen
            var dialog = new DeleteBrowserContentDialog
            {
                // 2. XamlRoot setzen (sonst stürzt WinUI 3 ab)
                XamlRoot = _xamlRoot
            };

            // 3. Anzeigen und warten, bis der Nutzer ihn schließt
            await dialog.ShowAsync();
        }

        public async Task ShowInstallAddOnDialogAsync()
        {
            // 1. Instanz erstellen
            var dialog = new InstallAddOnDialog
            {
                // 2. XamlRoot setzen (sonst stürzt WinUI 3 ab)
                XamlRoot = _xamlRoot
            };
            // 3. Anzeigen und warten, bis der Nutzer ihn schließt
            await dialog.ShowAsync();
        }
    }
}
