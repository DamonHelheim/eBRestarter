using CommunityToolkit.Mvvm.ComponentModel;
using eBRestarter.Core.Application.Ports.Outbound.Providers;
using eBRestarter.Core.Application.Providers;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using eBRestarter.Core.Application.Ports.Outbound.Config;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Core.Application.Ports.Outbound.Application;
using eBRestarter.Core.Application.Ports.Inbound.UseCases.RemoveApiCredentials;
using eBRestarter.Core.Application.UseCases.RemoveApiCredentials;
using eBRestarter.Core.Application.Models.Errors;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Domain.Entities;
using eBRestarter.Desktop.WinUI3.Messages;
using eBRestarter.Desktop.WinUI3.Models.Enums;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public sealed partial class ViewModelOptionsApi : ObservableObject
    {
        private readonly IDialogService _dialogService;
        private readonly IEVisitorConfigPort _EVRestarterConfigRepository;
        private readonly ILocalizationProvider _localizationService;
        private readonly IRemoveApiCredentialsUseCase _removeApiCredentialsUseCase;
        private readonly AppConfig _currentConfig;

        public ViewModelOptionsApi(
            IDialogService dialogService,
            IEVisitorConfigPort EVRestarterConfigRepository,
            ILocalizationProvider LocalizationProvider,
            IRemoveApiCredentialsUseCase removeApiCredentialsUseCase)
        {
            _dialogService = dialogService;
            _EVRestarterConfigRepository = EVRestarterConfigRepository;
            _localizationService = LocalizationProvider;
            _removeApiCredentialsUseCase = removeApiCredentialsUseCase;

            _currentConfig = _EVRestarterConfigRepository.LoadConfig();
        }

        [RelayCommand]
        private async Task RemoveAPICredentials()
        {
            _removeApiCredentialsUseCase.Execute();

            _currentConfig.Settings.ApiUsername = string.Empty;
            _currentConfig.Settings.ApiKey = string.Empty;

            WeakReferenceMessenger.Default.Send(new ApiCredentialsRemovedMessage());

            await _dialogService.ShowMessageAsync(
                _localizationService.RetrieveString("Options_RemoveCreds_Title"),
                _localizationService.RetrieveString("Options_RemoveCreds_Message"),
                DialogIcon.Success);
        }

        [RelayCommand]
        private async Task ShowActivateApiDialog()
        {
            await _dialogService.ShowActivateApiDialogAsync();
        }
    }
}







