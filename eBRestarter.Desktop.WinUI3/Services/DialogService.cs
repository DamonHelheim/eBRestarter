using eBRestarter.Core.Domain.Enums;
using eBRestarter.Core.Domain.Models;
using eBRestarter.Core.Domain.Models.Records;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using eBRestarter.Desktop.WinUI3.Views.Dialogs;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.Services
{
    public class DialogService : IDialogService
    {
        // Access to the main window's XamlRoot
        // WICHTIG: Stelle sicher, dass 'MainWindoweBRestarter' korrekt ist. 
        // Falls du App.MainWindow nutzt, passe es hier an.
        private XamlRoot XamlRoot => App.MainWindoweBRestarter!.Content.XamlRoot;

        // =========================================================
        // GENERIC HELPER METHOD
        // =========================================================
        // Instantiates any ContentDialog, sets XamlRoot, configures it, and shows it.
        private async Task<ContentDialogResult> ShowDialogInternalAsync<T>(Action<T>? configure = null) where T : ContentDialog, new()
        {
            var dialog = new T
            {
                XamlRoot = XamlRoot // Set centrally here
            };

            // Apply optional configuration (e.g., SetDefaults for AutoLogon or AutoStart logic)
            configure?.Invoke(dialog);

            return await dialog.ShowAsync();
        }

        // =========================================================
        // STANDARD DIALOGS (Manual instantiation required for Title/Content)
        // =========================================================

        public async Task<bool> ShowConfirmationAsync(string title, string message, string yesButtonText = "Ja", string noButtonText = "Nein")
        {
            if (App.MainWindoweBRestarter?.Content is FrameworkElement element)
            {
                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = message,
                    PrimaryButtonText = yesButtonText,
                    CloseButtonText = noButtonText,
                    XamlRoot = element.XamlRoot,
                    DefaultButton = ContentDialogButton.Primary
                };

                var result = await dialog.ShowAsync();

                // Rückgabe: true wenn "Ja" (Primary) geklickt wurde
                return result == ContentDialogResult.Primary;
            }
            return false;
        }

        public async Task ShowMessageAsync(string title, string message, DialogIcon icon = DialogIcon.None)
        {
            var dialog = new ContentDialog
            {
                XamlRoot = XamlRoot,
                Title = CreateTitleContent(title, icon),
                Content = message,
                CloseButtonText = "Schließen",
                DefaultButton = ContentDialogButton.Close
            };

            await dialog.ShowAsync();
        }

        public async Task<bool> ShowYesNoDialogAsync(string title, string message, DialogIcon icon = DialogIcon.Question)
        {
            var dialog = new ContentDialog
            {
                XamlRoot = XamlRoot,
                Title = CreateTitleContent(title, icon),
                Content = message,
                PrimaryButtonText = "Ja",
                CloseButtonText = "Nein",
                DefaultButton = ContentDialogButton.Primary
            };

            var result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        }

        // =========================================================
        // CUSTOM DIALOGS (Using the generic helper)
        // =========================================================

        public async Task ShowAboutDialogAsync() => await ShowDialogInternalAsync<AboutDialog>();

        public async Task ShowInstallAddOnDialogAsync() => await ShowDialogInternalAsync<InstallAddOnDialog>();

        public async Task ShowInstallAddOnInfoDialogAsync() => await ShowDialogInternalAsync<InstallAddOnInfoDialog>();

        public async Task ShowActivateApiDialogAsync() => await ShowDialogInternalAsync<ActivateApiDialog>();

        public async Task ShowImportApiDialogAsync() => await ShowDialogInternalAsync<ImportApiDialog>();

        public async Task ShowTurnOffEdgeStartupBoostDialogAsync() => await ShowDialogInternalAsync<TurnOffEdgeStartupBoostDialog>();

        // --- NEU & ANGEPASST: Browser Lösch-Dialog ---
        public async Task ShowDeleteBrowserContentDialogAsync(bool autoStart = false)
        {
            // Wir nutzen die generische Methode und konfigurieren den Dialog via Lambda
            await ShowDialogInternalAsync<DeleteBrowserContentDialog>(dialog =>
            {
                if (autoStart)
                {
                    // Wir abonnieren das "Opened"-Event, um den Prozess zu starten, sobald der Dialog sichtbar ist.
                    // Das ist sicherer als es direkt im Konstruktor zu machen.
                    dialog.Opened += async (s, e) =>
                    {
                        // Zugriff auf das ViewModel über die Property im Code-Behind des Dialogs
                        if (dialog.ViewModelDeleteBrowserContent != null)
                        {
                            await dialog.ViewModelDeleteBrowserContent.RunAutoSequenceAsync();
                        }
                    };
                }
            });
        }

        public async Task<AutoLogonDialogResult?> ShowAutoLogonDialogAsync(string defaultUser = null, string defaultDomain = null)
        {
            // We need a reference to the dialog instance to retrieve results later
            AutoLogonDialog dialogInstance = null!;

            // Use the generic method and configure via Lambda
            var result = await ShowDialogInternalAsync<AutoLogonDialog>(d =>
            {
                dialogInstance = d; // Capture instance
                d.SetDefaults(defaultUser, defaultDomain); // Call custom method
            });

            // Evaluate result
            if (result == ContentDialogResult.Primary)
            {
                return AutoLogonDialogResult.Save(dialogInstance.GetCredentials());
            }
            else if (result == ContentDialogResult.Secondary)
            {
                return AutoLogonDialogResult.Deactivate();
            }

            return null;
        }

        // =========================================================
        // HELPER: Build Title with Icon
        // =========================================================
        private object CreateTitleContent(string title, DialogIcon icon)
        {
            if (icon == DialogIcon.None) return title;

            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 12,
                VerticalAlignment = VerticalAlignment.Center
            };

            var fontIcon = new FontIcon
            {
                FontFamily = new FontFamily("Segoe Fluent Icons"),
                FontSize = 24,
                VerticalAlignment = VerticalAlignment.Center
            };

            switch (icon)
            {
                case DialogIcon.Error:
                    fontIcon.Glyph = "\uE783";
                    fontIcon.Foreground = new SolidColorBrush(Colors.Red);
                    break;
                case DialogIcon.Warning:
                    fontIcon.Glyph = "\uE7BA";
                    fontIcon.Foreground = new SolidColorBrush(Colors.Orange);
                    break;
                case DialogIcon.Information:
                    fontIcon.Glyph = "\uE946";
                    fontIcon.Foreground = new SolidColorBrush(Colors.CornflowerBlue);
                    break;
                case DialogIcon.Success:
                    fontIcon.Glyph = "\uE73E";
                    fontIcon.Foreground = new SolidColorBrush(Colors.LimeGreen);
                    break;
                case DialogIcon.Question:
                    fontIcon.Glyph = "\uE9CE";
                    break;
            }

            var textBlock = new TextBlock
            {
                Text = title,
                FontSize = 20,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center
            };

            stackPanel.Children.Add(fontIcon);
            stackPanel.Children.Add(textBlock);

            return stackPanel;
        }
    }
}

    //public class DialogService : IDialogService
    //{
    //    // Zugriff auf das XamlRoot des Hauptfensters
    //    private XamlRoot XamlRoot => App.MainWindoweBRestarter!.Content.XamlRoot;

    //    // ---------------------------------------------------------
    //    // STANDARD DIALOGE (Jetzt mit Icon Support)
    //    // ---------------------------------------------------------
    //    public async Task ShowMessageAsync(string title, string message, DialogIcon icon = DialogIcon.None)
    //    {
    //        var dialog = new ContentDialog
    //        {
    //            XamlRoot = XamlRoot,
    //            // HIER nutzen wir die Hilfsmethode:
    //            Title = CreateTitleContent(title, icon),
    //            Content = message,
    //            CloseButtonText = "Schließen",
    //            DefaultButton = ContentDialogButton.Close
    //        };

    //        await dialog.ShowAsync();
    //    }

    //    public async Task<bool> ShowYesNoDialogAsync(string title, string message, DialogIcon icon = DialogIcon.Question)
    //    {
    //        var dialog = new ContentDialog
    //        {
    //            XamlRoot = XamlRoot,
    //            // HIER nutzen wir die Hilfsmethode:
    //            Title = CreateTitleContent(title, icon),
    //            Content = message,
    //            PrimaryButtonText = "Ja",
    //            CloseButtonText = "Nein",
    //            DefaultButton = ContentDialogButton.Primary
    //        };

    //        var result = await dialog.ShowAsync();
    //        return result == ContentDialogResult.Primary;
    //    }

    //    // ---------------------------------------------------------
    //    // HILFSMETHODE: Baut den Titel mit Icon zusammen
    //    // ---------------------------------------------------------
    //    private object CreateTitleContent(string title, DialogIcon icon)
    //    {
    //        // Wenn kein Icon gewünscht ist, geben wir einfach den String zurück (Standard-Verhalten)
    //        if (icon == DialogIcon.None)
    //        {
    //            return title;
    //        }

    //        // StackPanel für Icon + Text nebeneinander
    //        var stackPanel = new StackPanel
    //        {
    //            Orientation = Orientation.Horizontal,
    //            Spacing = 12,
    //            VerticalAlignment = VerticalAlignment.Center
    //        };

    //        // Das Icon-Element
    //        var fontIcon = new FontIcon
    //        {
    //            FontFamily = new FontFamily("Segoe Fluent Icons"),
    //            FontSize = 24, // Etwas größer als der Text
    //            VerticalAlignment = VerticalAlignment.Center
    //        };

    //        // Icon und Farbe je nach Typ wählen
    //        switch (icon)
    //        {
    //            case DialogIcon.Error:
    //                fontIcon.Glyph = "\uE783"; // ErrorBadge
    //                fontIcon.Foreground = new SolidColorBrush(Colors.Red); // Oder besser: ThemeResource nutzen wenn möglich
    //                break;
    //            case DialogIcon.Warning:
    //                fontIcon.Glyph = "\uE7BA"; // Warning
    //                fontIcon.Foreground = new SolidColorBrush(Colors.Orange);
    //                break;
    //            case DialogIcon.Information:
    //                fontIcon.Glyph = "\uE946"; // Info
    //                fontIcon.Foreground = new SolidColorBrush(Colors.CornflowerBlue);
    //                break;
    //            case DialogIcon.Success:
    //                fontIcon.Glyph = "\uE73E"; // CheckMark
    //                fontIcon.Foreground = new SolidColorBrush(Colors.LimeGreen);
    //                break;
    //            case DialogIcon.Question:
    //                fontIcon.Glyph = "\uE9CE"; // Help/Question
    //                // Standard Textfarbe nehmen (kein Foreground setzen)
    //                break;
    //        }

    //        // Der Text-Titel
    //        var textBlock = new TextBlock
    //        {
    //            Text = title,
    //            FontSize = 20,
    //            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
    //            VerticalAlignment = VerticalAlignment.Center
    //        };

    //        stackPanel.Children.Add(fontIcon);
    //        stackPanel.Children.Add(textBlock);

    //        return stackPanel;
    //    }


    //    // ---------------------------------------------------------
    //    // CUSTOM DIALOGE (Bleiben unverändert, da XAML-basiert)
    //    // ---------------------------------------------------------
    //    public async Task<AutoLogonDialogResult?> ShowAutoLogonDialogAsync(string defaultUser = null, string defaultDomain = null)
    //    {
    //        var dialog = new AutoLogonDialog
    //        {
    //            XamlRoot = XamlRoot
    //        };

    //        dialog.SetDefaults(defaultUser, defaultDomain);

    //        var result = await dialog.ShowAsync();

    //        if (result == ContentDialogResult.Primary)
    //        {
    //            return AutoLogonDialogResult.Save(dialog.GetCredentials());
    //        }
    //        else if (result == ContentDialogResult.Secondary)
    //        {
    //            return AutoLogonDialogResult.Deactivate();
    //        }

    //        return null;
    //    }

    //    public async Task ShowAboutDialogAsync()
    //    {
    //        var dialog = new AboutDialog { XamlRoot = XamlRoot };
    //        await dialog.ShowAsync();
    //    }

    //    public async Task ShowBrowserDeleteContentDialogAsync()
    //    {
    //        var dialog = new DeleteBrowserContentDialog { XamlRoot = XamlRoot };
    //        await dialog.ShowAsync();
    //    }

    //    public async Task ShowInstallAddOnDialogAsync()
    //    {
    //        var dialog = new InstallAddOnDialog { XamlRoot = XamlRoot };
    //        await dialog.ShowAsync();
    //    }

    //    public async Task ShowActivateApiDialogAsync()
    //    {
    //        var dialog = new ActivateApiDialog { XamlRoot = XamlRoot };
    //        await dialog.ShowAsync();
    //    }

    //    public async Task ShowImportApiDialogAsync()
    //    {
    //        var dialog = new ImportApiDialog { XamlRoot = XamlRoot };
    //        await dialog.ShowAsync();
    //    }
    //}

