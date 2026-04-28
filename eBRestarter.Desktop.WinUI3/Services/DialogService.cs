using eBRestarter.Desktop.WinUI3.Models;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using eBRestarter.Desktop.WinUI3.Views.Dialogs;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Threading.Tasks;
using eBRestarter.Desktop.WinUI3.Models.Enums;

namespace eBRestarter.Desktop.WinUI3.Services;

public class DialogService : IDialogService
{
    // =========================================================
    // 1. FIELDS & INJECTED SERVICES (Backing state / XamlRoot access)
    // =========================================================
    #region FieldsAndInjectedServices

    private XamlRoot XamlRoot => App.MainWindoweBRestarter!.Content.XamlRoot;

    #endregion

    // =========================================================
    // 2. PUBLIC METHODS
    // =========================================================
    #region PublicMethods

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

    public async Task ShowAboutDialogAsync() => await ShowDialogInternalAsync<AboutDialog>();
    public async Task ShowInstallAddOnDialogAsync() => await ShowDialogInternalAsync<InstallAddOnDialog>();
    public async Task ShowInstallAddOnInfoDialogAsync() => await ShowDialogInternalAsync<InstallAddOnInfoDialog>();
    public async Task ShowActivateApiDialogAsync() => await ShowDialogInternalAsync<ActivateApiDialog>();
    public async Task ShowImportApiDialogAsync() => await ShowDialogInternalAsync<ImportApiDialog>();
    public async Task ShowTurnOffEdgeStartupBoostDialogAsync() => await ShowDialogInternalAsync<TurnOffEdgeStartupBoostDialog>();

    public async Task ShowDeleteBrowserContentDialogAsync(bool autoStart = false)
    {
        await ShowDialogInternalAsync<DeleteBrowserContentDialog>(dialog =>
        {
            if (autoStart)
            {
                dialog.Opened += async (s, e) =>
                {
                    if (dialog.ViewModelDeleteBrowserContent != null)
                        await dialog.ViewModelDeleteBrowserContent.RunAutoSequenceAsync();
                };
            }
        });
    }

    public async Task<AutoLogonDialogResult?> ShowAutoLogonDialogAsync(string defaultUser = null!, string defaultDomain = null!)
    {
        AutoLogonDialog dialogInstance = null!;
        var result = await ShowDialogInternalAsync<AutoLogonDialog>(d =>
        {
            dialogInstance = d;
            d.SetDefaults(defaultUser, defaultDomain);
        });
        if (result == ContentDialogResult.Primary)
            return AutoLogonDialogResult.Save(dialogInstance.GetCredentials());
        if (result == ContentDialogResult.Secondary)
            return AutoLogonDialogResult.Deactivate();
        return null;
    }

    #endregion

    // =========================================================
    // 3. PRIVATE HELPER METHODS (Internal helpers)
    // =========================================================
    #region PrivateHelperMethods

    private async Task<ContentDialogResult> ShowDialogInternalAsync<T>(Action<T>? configure = null) where T : ContentDialog, new()
    {
        var dialog = new T { XamlRoot = XamlRoot };
        configure?.Invoke(dialog);
        return await dialog.ShowAsync();
    }

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

    #endregion
}
