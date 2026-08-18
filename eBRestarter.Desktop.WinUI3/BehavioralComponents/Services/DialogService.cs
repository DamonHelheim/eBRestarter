using System;
using System.Threading.Tasks;

using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

using eBRestarter.Desktop.WinUI3.BehavioralComponents.Providers.Interfaces;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Services.Interfaces;
using eBRestarter.Desktop.WinUI3.ObjectArchetypes.Enums;
using eBRestarter.Desktop.WinUI3.ObjectArchetypes.Models.FormModels;
using eBRestarter.Desktop.WinUI3.VisualUIComponents.Views.Dialogs;

namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Services;

/// <summary>
/// Service implementation for presenting modal dialogs, message boxes, and specialized configuration dialogs in WinUI3.
/// </summary>
/// <param name="mainWindowProvider">Provider supplying access to the active main window instance.</param>
public sealed class DialogService(
    IMainWindowProvider mainWindowProvider) : IDialogService
{
    private const string ButtonCloseText = "Schließen";
    private const string ButtonNoText = "Nein";
    private const string ButtonYesText = "Ja";

    private const string FontFamilySegoeFluent = "Segoe Fluent Icons";

    private const string GlyphError = "\uE783";
    private const string GlyphInformation = "\uE946";
    private const string GlyphQuestion = "\uE9CE";
    private const string GlyphSuccess = "\uE73E";
    private const string GlyphWarning = "\uE9CE";

    private readonly IMainWindowProvider _mainWindowProvider = mainWindowProvider ?? throw new ArgumentNullException(nameof(mainWindowProvider));

    private XamlRoot XamlRoot => _mainWindowProvider.MainWindow.Content.XamlRoot;
    private ElementTheme CurrentTheme => (_mainWindowProvider.MainWindow.Content as FrameworkElement)?.RequestedTheme ?? ElementTheme.Default;

    /// <inheritdoc />
    public async Task<bool> ShowConfirmationAsync(
        string title,
        string message,
        string yesButtonText = ButtonYesText,
        string noButtonText = ButtonNoText)
    {
        if (_mainWindowProvider.MainWindow.Content is FrameworkElement element)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                PrimaryButtonText = yesButtonText,
                CloseButtonText = noButtonText,
                XamlRoot = element.XamlRoot,
                RequestedTheme = element.RequestedTheme,
                DefaultButton = ContentDialogButton.Primary
            };

            var result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        }

        return false;
    }

    /// <inheritdoc />
    public async Task ShowMessageAsync(
        string title,
        string message,
        DialogIcon icon = DialogIcon.None)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            RequestedTheme = CurrentTheme,
            Title = CreateTitleContent(title, icon),
            Content = message,
            CloseButtonText = ButtonCloseText,
            DefaultButton = ContentDialogButton.Close
        };

        await dialog.ShowAsync();
    }

    /// <inheritdoc />
    public async Task<bool> ShowYesNoDialogAsync(
        string title,
        string message,
        DialogIcon icon = DialogIcon.Question)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            RequestedTheme = CurrentTheme,
            Title = CreateTitleContent(title, icon),
            Content = message,
            PrimaryButtonText = ButtonYesText,
            CloseButtonText = ButtonNoText,
            DefaultButton = ContentDialogButton.Primary
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }

    /// <inheritdoc />
    public Task ShowAboutDialogAsync() => ShowDialogInternalAsync<AboutDialog>();

    /// <inheritdoc />
    public Task ShowInstallAddOnDialogAsync() => ShowDialogInternalAsync<InstallAddOnDialog>();

    /// <inheritdoc />
    public Task ShowInstallAddOnInfoDialogAsync() => ShowDialogInternalAsync<InstallAddOnInfoDialog>();

    /// <inheritdoc />
    public Task ShowActivateApiDialogAsync() => ShowDialogInternalAsync<ActivateApiDialog>();

    /// <inheritdoc />
    public Task ShowTurnOffEdgeStartupBoostDialogAsync() => ShowDialogInternalAsync<TurnOffEdgeStartupBoostDialog>();

    /// <inheritdoc />
    public Task ShowDeleteBrowserContentDialogAsync(bool shouldAutoStart = false)
    {
        return ShowDialogInternalAsync<DeleteBrowserContentDialog>(dialog =>
        {
            if (shouldAutoStart)
            {
                dialog.Opened += async (_, __) =>
                {
                    if (dialog.ViewModelDeleteBrowserContent != null)
                    {
                        await dialog.ViewModelDeleteBrowserContent.RunAutoSequenceAsync();
                    }
                };
            }
        });
    }

    /// <inheritdoc />
    public async Task<AutoLogonDialogResult?> ShowAutoLogonDialogAsync(
        string defaultUser = null!,
        string defaultDomain = null!,
        bool isPasswordlessEnabled = false,
        bool isAdmin = false)
    {
        AutoLogonDialog dialogInstance = null!;

        var result = await ShowDialogInternalAsync<AutoLogonDialog>(dialog =>
        {
            dialogInstance = dialog;
            dialog.SetDefaults(defaultUser, defaultDomain, isPasswordlessEnabled, isAdmin);
        });

        if (result == ContentDialogResult.Primary)
        {
            return AutoLogonDialogResult.Save(dialogInstance.GetCredentials(), dialogInstance.DisablePasswordlessMode);
        }

        if (result == ContentDialogResult.Secondary)
        {
            return AutoLogonDialogResult.Deactivate(dialogInstance.RestorePasswordlessMode);
        }

        return null;
    }

    /// <summary>
    /// Instantiates, configures, and displays a modal dialog asynchronously using current window XamlRoot and theme settings.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="ContentDialog"/> to display.</typeparam>
    /// <param name="configure">Optional configuration callback invoked prior to showing the dialog.</param>
    /// <returns>The <see cref="ContentDialogResult"/> indicating which button was clicked.</returns>
    private async Task<ContentDialogResult> ShowDialogInternalAsync<T>(Action<T>? configure = null) where T : ContentDialog, new()
    {
        var dialog = new T
        {
            XamlRoot = XamlRoot,
            RequestedTheme = CurrentTheme
        };

        configure?.Invoke(dialog);
        return await dialog.ShowAsync();
    }

    /// <summary>
    /// Creates styled title content containing an optional status icon and text block for dialog headers.
    /// </summary>
    /// <param name="title">The title header text.</param>
    /// <param name="icon">The status icon to render.</param>
    /// <returns>An object representing the title element (either string title or StackPanel with icon and text).</returns>
    private static object CreateTitleContent(string title, DialogIcon icon)
    {
        if (icon == DialogIcon.None)
        {
            return title;
        }

        var (glyph, foreground) = icon switch
        {
            DialogIcon.Error => (GlyphError, new SolidColorBrush(Colors.Red)),
            DialogIcon.Warning => (GlyphWarning, new SolidColorBrush(Colors.Orange)),
            DialogIcon.Information => (GlyphInformation, new SolidColorBrush(Colors.CornflowerBlue)),
            DialogIcon.Success => (GlyphSuccess, new SolidColorBrush(Colors.LimeGreen)),
            DialogIcon.Question => (GlyphQuestion, null as Brush),
            _ => (string.Empty, null as Brush)
        };

        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            VerticalAlignment = VerticalAlignment.Center
        };

        var fontIcon = new FontIcon
        {
            FontFamily = new FontFamily(FontFamilySegoeFluent),
            FontSize = 24,
            Glyph = glyph,
            Foreground = foreground,
            VerticalAlignment = VerticalAlignment.Center
        };

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
