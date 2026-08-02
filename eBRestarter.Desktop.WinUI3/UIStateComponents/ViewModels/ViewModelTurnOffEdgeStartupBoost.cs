using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Services.Interfaces;
using eBRestarter.Desktop.WinUI3.ObjectArchetypes.Enums;

namespace eBRestarter.Desktop.WinUI3.ViewModels;

/// <summary>
/// View model for the "Turn off Edge Startup Boost" dialog. Reads and toggles the Edge
/// Startup Boost setting via <see cref="IUseCaseToggleEdgeStartupBoost"/> and shows success/error
/// in an InfoBar. Provides a command to copy the settings URL for users who prefer to change it manually in Edge.
/// </summary>
public sealed partial class ViewModelTurnOffEdgeStartupBoost : ObservableObject
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitive Typen & Strings ──
    private const string BrowserNotInstalledResourceKey = "Browser_NotInstalled";
    private const string EdgeStartupBoostSettingsClipboardText = "edge://settings/?search=Startup-Boost";
    private const string StartupBoostDialogCopyMessageResourceKey = "StartupBoostDialog_CopyMessage";
    private const string StartupBoostDialogErrorTitleResourceKey = "StartupBoostDialog_ErrorTitle";
    private const string StartupBoostDialogSuccessMessageResourceKey = "StartupBoostDialog_SuccessMessage";
    private const string StartupBoostDialogSuccessStatusActivatedResourceKey = "StartupBoostDialog_SuccessStatus_Activated";
    private const string StartupBoostDialogSuccessTitleResourceKey = "StartupBoostDialog_SuccessTitle";
    private const string StartupBoostDialogTitleResourceKey = "StartupBoostDialog.Title";

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injizierte Abhängigkeiten (Dependencies) ──
    private readonly IDialogService _dialogService;
    private readonly IInboundPortLocalizationProvider _localizationService;
    private readonly IUseCaseToggleEdgeStartupBoost _toggleEdgeStartupBoostUseCase;
    private readonly IOutboundPortSystemInfoProvider _windowsSystemInfo;

    // ── Block 2: Primitive Typen & Strings ──
    private bool _isRevertingState;

    // ═══════════════════════════════════════════════════════
    //  3. Observable Properties (+ Partial Methods)
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitive Typen & Strings ──
    [ObservableProperty] public partial string InfoBarMessage { get; set; } = string.Empty;
    [ObservableProperty] public partial string InfoBarTitle { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotAdministrator))]
    public partial bool IsAdministrator { get; set; }

    [ObservableProperty] public partial bool IsInfoBarOpen { get; set; }
    [ObservableProperty] public partial bool IsStartupBoostEnabled { get; set; }

    /// <summary>
    /// When the user toggles the switch, applies the new value via the use case and shows
    /// success or error in the InfoBar. On failure, reverts the toggle so the UI matches the actual state.
    /// The source-generated partial uses the parameter name <c>value</c> (CommunityToolkit convention).
    /// </summary>
    async partial void OnIsStartupBoostEnabledChanged(bool value)
    {
        if (_isRevertingState)
        {
            return;
        }

        IsInfoBarOpen = false;

        var toggleResponse = _toggleEdgeStartupBoostUseCase.Toggle(value);

        if (toggleResponse.Success)
        {
            InfoBarTitle = _localizationService.RetrieveString(StartupBoostDialogSuccessTitleResourceKey);
            InfoBarMessage = toggleResponse.NewState
                ? _localizationService.RetrieveString(StartupBoostDialogSuccessStatusActivatedResourceKey)
                : _localizationService.RetrieveString(StartupBoostDialogSuccessMessageResourceKey);
            InfoBarSeverity = InfoBarSeverity.Success;
        }
        else
        {
            _isRevertingState = true;
            IsStartupBoostEnabled = toggleResponse.NewState;
            _isRevertingState = false;

            InfoBarTitle = _localizationService.RetrieveString(StartupBoostDialogErrorTitleResourceKey);
            InfoBarMessage = toggleResponse.ErrorMessage ?? string.Empty;
            InfoBarSeverity = InfoBarSeverity.Error;
        }

        IsInfoBarOpen = true;
    }

    // ── Block 4: Komplexe Typen, Collections & UI-Elemente ──
    [ObservableProperty] public partial InfoBarSeverity InfoBarSeverity { get; set; } = InfoBarSeverity.Informational;


    // ═══════════════════════════════════════════════════════
    //  4. Properties
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitive Typen & Strings ──
    public bool IsNotAdministrator => !IsAdministrator;

    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Initializes the VM with startup and dialog services and reads the current Edge Startup Boost
    /// state so the toggle reflects the actual setting when the dialog opens.
    /// </summary>
    public ViewModelTurnOffEdgeStartupBoost(
        IDialogService dialogService,
        IInboundPortLocalizationProvider localizationService,
        IUseCaseToggleEdgeStartupBoost toggleEdgeStartupBoostUseCase,
        IOutboundPortSystemInfoProvider windowsSystemInfo)
    {
        ArgumentNullException.ThrowIfNull(dialogService);
        ArgumentNullException.ThrowIfNull(localizationService);
        ArgumentNullException.ThrowIfNull(toggleEdgeStartupBoostUseCase);
        ArgumentNullException.ThrowIfNull(windowsSystemInfo);

        _dialogService = dialogService;
        _localizationService = localizationService;
        _toggleEdgeStartupBoostUseCase = toggleEdgeStartupBoostUseCase;
        _windowsSystemInfo = windowsSystemInfo;

        IsAdministrator = _windowsSystemInfo.IsUserAdministrator();
        IsStartupBoostEnabled = _toggleEdgeStartupBoostUseCase.IsEnabled();
    }

    // ═══════════════════════════════════════════════════════
    //  7. Commands
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Copies the Edge settings URL (Startup Boost) to the clipboard and shows an info message.
    /// If Edge is not installed, shows an error instead. Lets the user open Edge manually to change the setting.
    /// </summary>
    [RelayCommand]
    private async Task CopyAndOpenEdgeAsync()
    {
        string dialogTitle = _localizationService.RetrieveString(StartupBoostDialogTitleResourceKey);

        if (_toggleEdgeStartupBoostUseCase.IsEdgeInstalled())
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(EdgeStartupBoostSettingsClipboardText);
            Clipboard.SetContent(dataPackage);

            await _dialogService.ShowMessageAsync(
                dialogTitle,
                _localizationService.RetrieveString(StartupBoostDialogCopyMessageResourceKey),
                DialogIcon.Information);
        }
        else
        {
            await _dialogService.ShowMessageAsync(
                dialogTitle,
                _localizationService.RetrieveString(BrowserNotInstalledResourceKey),
                DialogIcon.Error);
        }
    }
}
