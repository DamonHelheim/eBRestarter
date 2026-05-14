using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBRestarter.Core.Application.Constants;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.Models.Config;
using eBRestarter.Core.Domain.Entities;
using eBRestarter.Desktop.WinUI3.Models;
using eBRestarter.Desktop.WinUI3.Models.Enums;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public partial class ViewModelOptionsExtension : ObservableObject
    {
        private const string ExtensionConfigFileName = "tab_restarter_config.json";
        private const string GeneralErrorKey = "General_Error";
        private const int ExtensionSaveStatusDisplayDurationMilliseconds = 3000;
        private const int MillisecondsPerMinute = 60000;

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            WriteIndented = true,
            TypeInfoResolver = ExtensionConfigJsonContext.Default
        };

        private readonly IBrowserExtensionDeploymentService _browserExtensionDeploymentService;
        private readonly IDialogService _dialogService;
        private readonly IEVisitorConfigService _eVisitorConfigService;
        private readonly ILocalizationService _localizationService;
        private readonly IOperatingSystemFacade _operatingSystemFacade;
        private readonly IUIOptionsService _uiOptionsService;
        private readonly AppConfig _currentConfig;

        [ObservableProperty]
        public partial string ExtensionSaveStatus { get; set; } = string.Empty;

        [ObservableProperty]
        public partial double ExtensionWaitTimeMinutes { get; set; } = 3;

        [ObservableProperty]
        public partial string ExtensionUrl { get; set; } = string.Empty;

        [ObservableProperty]
        public partial LanguageOption SelectedExtensionLanguage { get; set; }

        public ReadOnlyCollection<LanguageOption> ExtensionLanguages { get; }

        public ViewModelOptionsExtension(
            IBrowserExtensionDeploymentService browserExtensionDeploymentService,
            IDialogService dialogService,
            IEVisitorConfigService eVisitorConfigService,
            ILocalizationService localizationService,
            IOperatingSystemFacade operatingSystemFacade,
            IUIOptionsService uiOptionsService)
        {
            _browserExtensionDeploymentService = browserExtensionDeploymentService;
            _dialogService = dialogService;
            _eVisitorConfigService = eVisitorConfigService;
            _localizationService = localizationService;
            _operatingSystemFacade = operatingSystemFacade;
            _uiOptionsService = uiOptionsService;

            _currentConfig = _eVisitorConfigService.LoadConfig();

            _browserExtensionDeploymentService.EnsureExtensionIsDeployed();

            ExtensionLanguages = new ReadOnlyCollection<LanguageOption>([.. _uiOptionsService.GetAvailableLanguages()]);
            if (ExtensionLanguages.Count == 0)
            {
                throw new InvalidOperationException("Requires at least one language entry.");
            }

            SelectedExtensionLanguage = ExtensionLanguages[0];

            LoadExtensionConfig();
        }

        [RelayCommand]
        private async Task OpenExtensionFolder()
        {
            try
            {
                string extensionPath = _browserExtensionDeploymentService.RetrieveExtensionFolderPath();

                if (!Directory.Exists(extensionPath))
                {
                    Directory.CreateDirectory(extensionPath);
                }

                _operatingSystemFacade.WindowsProcessControlService.OpenExplorer(extensionPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                string messageFormat = _localizationService.RetrieveString("Options_ExtensionFolderOpenError_Message");
                await _dialogService.ShowMessageAsync(
                    _localizationService.RetrieveString("Options_ExtensionFolderOpenError_Title"),
                    string.Format(messageFormat, ex.Message),
                    DialogIcon.Error);
            }
        }

        [RelayCommand]
        private async Task SaveExtensionConfig()
        {
            try
            {
                string extensionPath = _browserExtensionDeploymentService.RetrieveExtensionFolderPath();
                string configPath = Path.Combine(extensionPath, ExtensionConfigFileName);

                var extensionConfigDto = new ExtensionConfigDto
                {
                    LANGUAGE = SelectedExtensionLanguage?.Index == 0 ? "DE" : "EN",
                    ZIEL_URL = $"{WebLinks.EVisitorSurflink}{_currentConfig.Username}",
                    WARTEZEIT_MS = (int)(ExtensionWaitTimeMinutes * MillisecondsPerMinute)
                };

                string jsonString = JsonSerializer.Serialize(extensionConfigDto, _jsonSerializerOptions);

                if (!Directory.Exists(extensionPath))
                {
                    Directory.CreateDirectory(extensionPath);
                }

                await File.WriteAllTextAsync(configPath, jsonString);

                ExtensionSaveStatus = _localizationService.RetrieveString("Options_SavedSuccessfully") ?? string.Empty;
                await Task.Delay(ExtensionSaveStatusDisplayDurationMilliseconds);
                ExtensionSaveStatus = string.Empty;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await _dialogService.ShowMessageAsync(
                    _localizationService.RetrieveString(GeneralErrorKey),
                    _localizationService.RetrieveString("BrowserExtension_Config_Error_Message") + " " + ex.Message,
                    DialogIcon.Error);
            }
        }

        private void LoadExtensionConfig()
        {
            try
            {
                string configPath = Path.Combine(_browserExtensionDeploymentService.RetrieveExtensionFolderPath(), ExtensionConfigFileName);

                if (File.Exists(configPath))
                {
                    string jsonString = File.ReadAllText(configPath);
                    var extensionConfigDto = JsonSerializer.Deserialize(jsonString, ExtensionConfigJsonContext.Default.ExtensionConfigDto);

                    if (extensionConfigDto != null)
                    {
                        ExtensionUrl = extensionConfigDto.ZIEL_URL;
                        ExtensionWaitTimeMinutes = extensionConfigDto.WARTEZEIT_MS / (double)MillisecondsPerMinute;

                        LanguageOption? matchingExtensionLanguage = extensionConfigDto.LANGUAGE == "EN"
                            ? ExtensionLanguages.FirstOrDefault(languageOption => languageOption.Index == 1)
                            : ExtensionLanguages.FirstOrDefault(languageOption => languageOption.Index == 0);

                        if (matchingExtensionLanguage != null)
                        {
                            SelectedExtensionLanguage = matchingExtensionLanguage;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Could not read existing tab_restarter_config.json: {ex}");
            }
        }
    }
}
