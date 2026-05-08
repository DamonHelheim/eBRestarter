using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.UseCases.ToggleEdgeStartupBoost;
using eBRestarter.Desktop.WinUI3.Models.Enums;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace eBRestarter.Desktop.WinUI3.ViewModels;

/// <summary>
/// View model for the "Turn off Edge Startup Boost" dialog. Reads and toggles the Edge
/// Startup Boost setting via <see cref="IToggleEdgeStartupBoostUseCase"/> and shows success/error
/// in an InfoBar. Provides a command to copy the settings URL for users who prefer to change it manually in Edge.
/// </summary>
public partial class ViewModelTurnOffEdgeStartupBoost : ObservableObject
{
    private const string EdgeStartupBoostSettingsClipboardText = "edge://settings/?search=Startup-Boost";

    private readonly IDialogService _dialogService;
    private readonly ILocalizationService _localizationService;
    private readonly IToggleEdgeStartupBoostUseCase _toggleEdgeStartupBoostUseCase;
    private readonly IOperatingSystemFacade _operatingSystemFacade;

    private bool _isRevertingState;

    [ObservableProperty]
    public partial string InfoBarMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string InfoBarTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsInfoBarOpen { get; set; }

    [ObservableProperty]
    public partial bool IsStartupBoostEnabled { get; set; }


    [ObservableProperty]
    public partial InfoBarSeverity InfoBarSeverity { get; set; } = InfoBarSeverity.Informational;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotAdministrator))]
    public partial bool IsAdministrator { get; set; }

    public bool IsNotAdministrator => !IsAdministrator;

    /// <summary>
    /// Initializes the VM with startup and dialog services and reads the current Edge Startup Boost
    /// state so the toggle reflects the actual setting when the dialog opens.
    /// </summary>
    public ViewModelTurnOffEdgeStartupBoost(
        IToggleEdgeStartupBoostUseCase toggleEdgeStartupBoostUseCase,
        IDialogService dialogService,
        ILocalizationService localizationService,
        IOperatingSystemFacade operatingSystemFacade)
    {
        ArgumentNullException.ThrowIfNull(toggleEdgeStartupBoostUseCase);
        ArgumentNullException.ThrowIfNull(dialogService);
        ArgumentNullException.ThrowIfNull(localizationService);
        ArgumentNullException.ThrowIfNull(operatingSystemFacade);

        _toggleEdgeStartupBoostUseCase = toggleEdgeStartupBoostUseCase;
        _dialogService = dialogService;
        _localizationService = localizationService;
        _operatingSystemFacade = operatingSystemFacade;

        IsAdministrator = _operatingSystemFacade.WindowsSystemInfoService.IsUserAdministrator();
        IsStartupBoostEnabled = _toggleEdgeStartupBoostUseCase.IsEnabled();
    }

    /// <summary>
    /// Copies the Edge settings URL (Startup Boost) to the clipboard and shows an info message.
    /// If Edge is not installed, shows an error instead. Lets the user open Edge manually to change the setting.
    /// </summary>
    [RelayCommand]
    private async Task CopyAndOpenEdge()
    {
        string dialogTitle = _localizationService.GetString("StartupBoostDialog.Title");

        if (_toggleEdgeStartupBoostUseCase.IsEdgeInstalled())
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(EdgeStartupBoostSettingsClipboardText);
            Clipboard.SetContent(dataPackage);

            await _dialogService.ShowMessageAsync(
                dialogTitle,
                _localizationService.GetString("StartupBoostDialog_CopyMessage"),
                DialogIcon.Information);
        }
        else
        {
            await _dialogService.ShowMessageAsync(
                dialogTitle,
                _localizationService.GetString("Browser_NotInstalled"),
                DialogIcon.Error);
        }
    }

    /// <summary>
    /// When the user toggles the switch, applies the new value via the use case and shows
    /// success or error in the InfoBar. On failure, reverts the toggle so the UI matches the actual state.
    /// The source-generated partial uses the parameter name <c>value</c> (CommunityToolkit convention).
    /// </summary>
    async partial void OnIsStartupBoostEnabledChanged(bool value)
    {
        if (_isRevertingState)
            return;

        IsInfoBarOpen = false;

        var toggleResponse = _toggleEdgeStartupBoostUseCase.Toggle(value);

        if (toggleResponse.Success)
        {
            InfoBarTitle = _localizationService.GetString("StartupBoostDialog_SuccessTitle");
            InfoBarMessage = toggleResponse.NewState
                ? _localizationService.GetString("StartupBoostDialog_SuccessStatus_Activated")
                : _localizationService.GetString("StartupBoostDialog_SuccessMessage");
            InfoBarSeverity = InfoBarSeverity.Success;
        }
        else
        {
            _isRevertingState = true;
            IsStartupBoostEnabled = toggleResponse.NewState;
            _isRevertingState = false;

            InfoBarTitle = _localizationService.GetString("StartupBoostDialog_ErrorTitle");
            InfoBarMessage = toggleResponse.ErrorMessage ?? string.Empty;
            InfoBarSeverity = InfoBarSeverity.Error;
        }

        IsInfoBarOpen = true;
    }
}
