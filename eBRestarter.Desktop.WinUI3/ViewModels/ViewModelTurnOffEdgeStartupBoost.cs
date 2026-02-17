using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Domain.Enums;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    /// <summary>
    /// View model for the "Turn off Edge Startup Boost" dialog. Reads and toggles the Edge
    /// Startup Boost setting via <see cref="IWindowsStartupManagerService"/> and shows success/error
    /// in an InfoBar. Provides a command to copy the settings URL and open Edge for users who
    /// prefer to change it manually.
    /// </summary>
    public partial class ViewModelTurnOffEdgeStartupBoost : ObservableObject
    {
        // =========================================================
        // 1. FIELDS & INJECTED SERVICES (Backing-Felder und DI)
        // =========================================================
        #region FieldsAndInjectedServices

        private readonly IWindowsStartupManagerService _startupService;
        private readonly IBrowserFactory _browserFactory;
        private readonly IDialogService _dialogService;
        private readonly ILocalizationService _localizationService;
        private bool _isRevertingState = false;

        #endregion

        // =========================================================
        // 2. OBSERVABLE PROPERTIES (MVVM State)
        // =========================================================
        #region ObservableProperties

        [ObservableProperty] public partial bool IsStartupBoostEnabled { get; set; }
        [ObservableProperty] public partial bool IsInfoBarOpen { get; set; } = false;
        [ObservableProperty] public partial string InfoBarTitle { get; set; } = string.Empty;
        [ObservableProperty] public partial string InfoBarMessage { get; set; } = string.Empty;
        [ObservableProperty] public partial InfoBarSeverity InfoBarSeverity { get; set; } = InfoBarSeverity.Informational;

        #endregion

        // =========================================================
        // 3. CONSTRUCTOR & FINALIZER (Ctor)
        // =========================================================
        #region ConstructorAndFinalizer

        /// <summary>
        /// Initializes the VM with startup and dialog services and reads the current Edge Startup Boost
        /// state so the toggle reflects the actual setting when the dialog opens.
        /// </summary>
        public ViewModelTurnOffEdgeStartupBoost(
            IWindowsStartupManagerService startupService,
            IBrowserFactory browserFactory,
            IDialogService dialogService,
            ILocalizationService localizationService)
        {
            _startupService = startupService;
            _browserFactory = browserFactory;
            _dialogService = dialogService;
            _localizationService = localizationService;
            IsStartupBoostEnabled = _startupService.IsEdgeStartupBoostEnabled();
        }

        #endregion

        // =========================================================
        // 4. COMMANDS (MVVM Actions)
        // =========================================================
        #region Commands

        /// <summary>
        /// Copies the Edge settings URL (Startup Boost) to the clipboard and shows an info message.
        /// If Edge is not installed, shows an error instead. Lets the user open Edge manually to change the setting.
        /// </summary>
        [RelayCommand]
        private async Task CopyAndOpenEdge()
        {
            var edge = _browserFactory.Create(BrowserType.Edge);
            if (edge.IsInstalled)
            {
                var dataPackage = new DataPackage();
                dataPackage.SetText("edge://settings/?search=Startup-Boost");
                Clipboard.SetContent(dataPackage);
                await _dialogService.ShowMessageAsync(
                    "Edge",
                    _localizationService.GetString("StartupBoostDialog_CopyMessage"),
                    DialogIcon.Information);
            }
            else
            {
                await _dialogService.ShowMessageAsync(
                    "Edge",
                    _localizationService.GetString("Browser_NotInstalled"),
                    DialogIcon.Error);
            }
        }

        #endregion

        // =========================================================
        // 5. PROPERTY CHANGE HANDLERS (MVVM Hooks)
        // =========================================================
        #region PropertyChangeHandlers

        /// <summary>
        /// When the user toggles the switch, applies the new value via the startup service and shows
        /// success or error in the InfoBar. On failure, reverts the toggle so the UI matches the actual state.
        /// </summary>
        async partial void OnIsStartupBoostEnabledChanged(bool value)
        {
            if (_isRevertingState) return;
            try
            {
                IsInfoBarOpen = false;
                _startupService.SetEdgeStartupBoost(value);
                InfoBarTitle = _localizationService.GetString("StartupBoostDialog_SuccessTitle");
                InfoBarMessage = value
                    ? _localizationService.GetString("StartupBoostDialog_SuccessStatus_Activated")
                    : _localizationService.GetString("StartupBoostDialog_SuccessMessage");
                InfoBarSeverity = InfoBarSeverity.Success;
                IsInfoBarOpen = true;
            }
            catch (Exception ex)
            {
                _isRevertingState = true;
                IsStartupBoostEnabled = !value;
                _isRevertingState = false;
                InfoBarTitle = _localizationService.GetString("StartupBoostDialog_ErrorTitle");
                InfoBarMessage = ex.Message;
                InfoBarSeverity = InfoBarSeverity.Error;
                IsInfoBarOpen = true;
            }
        }

        #endregion
    }
}
