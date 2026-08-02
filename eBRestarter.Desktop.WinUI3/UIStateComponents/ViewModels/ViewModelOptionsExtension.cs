using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Core.Domain.ValueObjects;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Providers.Interfaces;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Services.Interfaces;
using eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.UIOptionDTO;
using eBRestarter.Desktop.WinUI3.ObjectArchetypes.Enums;
using eBRestarter.Infrastructure.Common.Statics;
using eBRestarter.Infrastructure.ObjectArchetypes.DTOs;

namespace eBRestarter.Desktop.WinUI3.ViewModels;

public sealed partial class ViewModelOptionsExtension : ObservableObject
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitive Typen & Strings ──
    private const string ExtensionConfigErrorMessageResourceKey = "BrowserExtension_Config_Error_Message";
    private const string ExtensionConfigFileName = "tab_restarter_config.json";
    private const string ExtensionFolderOpenErrorMessageResourceKey = "Options_ExtensionFolderOpenError_Message";
    private const string ExtensionFolderOpenErrorTitleResourceKey = "Options_ExtensionFolderOpenError_Title";
    private const string ExtensionLanguageCodeEnglish = "EN";
    private const string ExtensionLanguageCodeGerman = "DE";
    private const int ExtensionSaveStatusDisplayDurationMilliseconds = 3000;
    private const string GeneralErrorKey = "General_Error";
    private const int MillisecondsPerMinute = 60000;
    private const string OptionsSavedSuccessfullyResourceKey = "Options_SavedSuccessfully";

    // ── Block 4: Komplexe Typen, Collections & UI-Elemente ──
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true,
        TypeInfoResolver = ExtensionConfigJsonContext.Default
    };

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injizierte Abhängigkeiten (Dependencies) ──
    private readonly IOutboundPortBrowserExtensionDeployment _browserExtensionDeploymentService;
    private readonly IOutboundPortBrowserExtensionPathProvider _browserExtensionPathProvider;
    private readonly IDialogService _dialogService;
    private readonly IOutboundPortEVisitorConfigRepository _evRestarterConfigRepository;
    private readonly IInboundPortLocalizationProvider _localizationService;
    private readonly IOutboundPortOsProcessControl _osProcessControlPort;
    private readonly IUIOptionsProvider _uiOptionsService;

    // ── Block 4: Komplexe Typen, Collections & UI-Elemente ──
    private readonly AppConfig? _currentConfig;

    // ═══════════════════════════════════════════════════════
    //  3. Observable Properties
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitive Typen & Strings ──
    [ObservableProperty] public partial string ExtensionSaveStatus { get; set; } = string.Empty;
    [ObservableProperty] public partial string ExtensionUrl { get; set; } = string.Empty;
    [ObservableProperty] public partial double ExtensionWaitTimeMinutes { get; set; } = 3;

    // ── Block 4: Komplexe Typen, Collections & UI-Elemente ──
    [ObservableProperty] public partial LanguageOption SelectedExtensionLanguage { get; set; }


    // ═══════════════════════════════════════════════════════
    //  4. Properties
    // ═══════════════════════════════════════════════════════
    public ReadOnlyCollection<LanguageOption> ExtensionLanguages { get; }

    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════
    public ViewModelOptionsExtension(
        IOutboundPortBrowserExtensionDeployment browserExtensionDeploymentService,
        IOutboundPortBrowserExtensionPathProvider browserExtensionPathProvider,
        IDialogService dialogService,
        IOutboundPortEVisitorConfigRepository evRestarterConfigRepository,
        IInboundPortLocalizationProvider localizationService,
        IOutboundPortOsProcessControl osProcessControlPort,
        IUIOptionsProvider uiOptionsService)
    {
        ArgumentNullException.ThrowIfNull(browserExtensionDeploymentService);
        ArgumentNullException.ThrowIfNull(browserExtensionPathProvider);
        ArgumentNullException.ThrowIfNull(dialogService);
        ArgumentNullException.ThrowIfNull(evRestarterConfigRepository);
        ArgumentNullException.ThrowIfNull(localizationService);
        ArgumentNullException.ThrowIfNull(osProcessControlPort);
        ArgumentNullException.ThrowIfNull(uiOptionsService);

        _browserExtensionDeploymentService = browserExtensionDeploymentService;
        _browserExtensionPathProvider = browserExtensionPathProvider;
        _dialogService = dialogService;
        _evRestarterConfigRepository = evRestarterConfigRepository;
        _localizationService = localizationService;
        _osProcessControlPort = osProcessControlPort;
        _uiOptionsService = uiOptionsService;

        _currentConfig = _evRestarterConfigRepository.LoadConfig();

        _browserExtensionDeploymentService.EnsureExtensionIsDeployed();

        ExtensionLanguages = new ReadOnlyCollection<LanguageOption>([.. _uiOptionsService.GetAvailableLanguages()]);
        if (ExtensionLanguages.Count == 0)
        {
            throw new InvalidOperationException("Requires at least one language entry.");
        }

        SelectedExtensionLanguage = ExtensionLanguages[0];

        LoadExtensionConfig();
    }

    // ═══════════════════════════════════════════════════════
    //  7. Commands
    // ═══════════════════════════════════════════════════════
    [RelayCommand]
    private async Task OpenExtensionFolderAsync()
    {
        try
        {
            string extensionPath = _browserExtensionPathProvider.RetrieveExtensionFolderPath();

            if (!Directory.Exists(extensionPath))
            {
                Directory.CreateDirectory(extensionPath);
            }

            _osProcessControlPort.OpenDirectoryInFileBrowser(extensionPath);
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
            string messageFormat = _localizationService.RetrieveString(ExtensionFolderOpenErrorMessageResourceKey);
            await _dialogService.ShowMessageAsync(
                _localizationService.RetrieveString(ExtensionFolderOpenErrorTitleResourceKey),
                string.Format(messageFormat, exception.Message),
                DialogIcon.Error);
        }
    }

    [RelayCommand]
    private async Task SaveExtensionConfigAsync()
    {
        try
        {
            string extensionPath = _browserExtensionPathProvider.RetrieveExtensionFolderPath();
            string configPath = Path.Combine(extensionPath, ExtensionConfigFileName);

            var extensionConfigDto = new ExtensionConfig
            {
                LANGUAGE = SelectedExtensionLanguage?.Index == 0 ? ExtensionLanguageCodeGerman : ExtensionLanguageCodeEnglish,
                ZIEL_URL = $"{WebLinks.EVisitorSurflink}{_currentConfig?.Username ?? string.Empty}",
                WARTEZEIT_MS = (int)(ExtensionWaitTimeMinutes * MillisecondsPerMinute)
            };

            string jsonString = JsonSerializer.Serialize(extensionConfigDto, _jsonSerializerOptions);

            if (!Directory.Exists(extensionPath))
            {
                Directory.CreateDirectory(extensionPath);
            }

            await File.WriteAllTextAsync(configPath, jsonString);

            ExtensionSaveStatus = _localizationService.RetrieveString(OptionsSavedSuccessfullyResourceKey) ?? string.Empty;
            await Task.Delay(ExtensionSaveStatusDisplayDurationMilliseconds);
            ExtensionSaveStatus = string.Empty;
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
            await _dialogService.ShowMessageAsync(
                _localizationService.RetrieveString(GeneralErrorKey),
                _localizationService.RetrieveString(ExtensionConfigErrorMessageResourceKey) + " " + exception.Message,
                DialogIcon.Error);
        }
    }

    // ═══════════════════════════════════════════════════════
    //  8. Methods (public → private)
    // ═══════════════════════════════════════════════════════
    private void LoadExtensionConfig()
    {
        try
        {
            string configPath = Path.Combine(_browserExtensionPathProvider.RetrieveExtensionFolderPath(), ExtensionConfigFileName);

            if (!File.Exists(configPath))
            {
                return;
            }

            string jsonString = File.ReadAllText(configPath);
            var extensionConfigDto = JsonSerializer.Deserialize(jsonString, ExtensionConfigJsonContext.Default.ExtensionConfig);

            if (extensionConfigDto is null)
            {
                return;
            }

            ExtensionUrl = extensionConfigDto.ZIEL_URL;
            ExtensionWaitTimeMinutes = extensionConfigDto.WARTEZEIT_MS / (double)MillisecondsPerMinute;

            LanguageOption? matchingExtensionLanguage = extensionConfigDto.LANGUAGE == ExtensionLanguageCodeEnglish
                ? ExtensionLanguages.FirstOrDefault(languageOption => languageOption.Index == 1)
                : ExtensionLanguages.FirstOrDefault(languageOption => languageOption.Index == 0);

            if (matchingExtensionLanguage is not null)
            {
                SelectedExtensionLanguage = matchingExtensionLanguage;
            }
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"Could not read existing tab_restarter_config.json: {exception}");
        }
    }
}
