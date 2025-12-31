using eBRestarter.Desktop.WinUI3.Services.Interfaces;
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
        private XamlRoot XamlRoot => App.MainWindoweBRestarter!.Content.XamlRoot;

        public Task ShowMessageAsync(string title, string message)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ShowYesNoDialogAsync(string title, string message)
        {
            var dialog = new ContentDialog
            {
                XamlRoot = XamlRoot,
                Title = title,
                Content = message,
                PrimaryButtonText = "Ja",
                CloseButtonText = "Nein"
            };
            var result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        }

        // ... Implementierung für ShowMessageAsync ähnlich ...
    }
}
