using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Threading.Tasks;

using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;
using eBRestarter.Core.Domain.ValueObjects;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Services.Interfaces;
using eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.SignalDTO.Messages;
using eBRestarter.Desktop.WinUI3.ObjectArchetypes.Enums;

namespace eBRestarter.Desktop.WinUI3.ViewModels;

/// <summary>
/// View model for the API options page. Provides commands to remove saved API credentials
/// and to open the API activation dialog.
/// </summary>
public sealed partial class ViewModelOptionsApi : ObservableObject
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    private const string RemoveCredentialsMessageResourceKey = "Options_RemoveCreds_Message";
    private const string RemoveCredentialsTitleResourceKey = "Options_RemoveCreds_Title";

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    private readonly IDialogService _dialogService;
    private readonly IOutboundPortEVisitorConfigRepository _evRestarterConfigRepository;
    private readonly IInboundPortLocalizationProvider _localizationService;
    private readonly IUseCaseRemoveApiCredentials _removeApiCredentialsUseCase;

    private readonly AppConfig? _currentConfig;

    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Initializes the VM with dialog, config, localization, and credential removal services,
    /// and loads the current application configuration.
    /// </summary>
    public ViewModelOptionsApi(
        IDialogService dialogService,
        IOutboundPortEVisitorConfigRepository evRestarterConfigRepository,
        IInboundPortLocalizationProvider localizationService,
        IUseCaseRemoveApiCredentials removeApiCredentialsUseCase)
    {
        ArgumentNullException.ThrowIfNull(dialogService);
        ArgumentNullException.ThrowIfNull(evRestarterConfigRepository);
        ArgumentNullException.ThrowIfNull(localizationService);
        ArgumentNullException.ThrowIfNull(removeApiCredentialsUseCase);

        _dialogService = dialogService;
        _evRestarterConfigRepository = evRestarterConfigRepository;
        _localizationService = localizationService;
        _removeApiCredentialsUseCase = removeApiCredentialsUseCase;

        _currentConfig = _evRestarterConfigRepository.LoadConfig();
    }

    // ═══════════════════════════════════════════════════════
    //  7. Commands
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Executes the credential removal use case, clears credentials in current config, sends a removal signal,
    /// and displays a success confirmation dialog.
    /// </summary>
    [RelayCommand]
    private async Task RemoveApiCredentialsAsync()
    {
        _removeApiCredentialsUseCase.Execute();

        if (_currentConfig?.Settings is not null)
        {
            _currentConfig.Settings.ApiUsername = string.Empty;
            _currentConfig.Settings.ApiKey = string.Empty;
        }

        WeakReferenceMessenger.Default.Send(new ApiCredentialsRemovedMessage());

        await _dialogService.ShowMessageAsync(
            _localizationService.RetrieveString(RemoveCredentialsTitleResourceKey),
            _localizationService.RetrieveString(RemoveCredentialsMessageResourceKey),
            DialogIcon.Success);
    }

    /// <summary>Opens the API activation dialog via the dialog service.</summary>
    [RelayCommand]
    private Task ShowActivateApiDialogAsync() => _dialogService.ShowActivateApiDialogAsync();
}
