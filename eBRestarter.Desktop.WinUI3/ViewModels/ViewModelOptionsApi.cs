using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using eBRestarter.Core.Application.Ports.Outbound.Config;
using eBRestarter.Core.Application.Ports.Inbound.Providers;
using eBRestarter.Core.Domain.Entities;
using eBRestarter.Desktop.WinUI3.Messages;
using eBRestarter.Desktop.WinUI3.Models.Enums;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using System;
using System.Threading.Tasks;
using eBRestarter.Core.Application.Ports.Inbound.UseCases;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public sealed partial class ViewModelOptionsApi : ObservableObject
    {
        private readonly IDialogService _dialogService;
        private readonly IEVisitorConfigRepositoryOutboundPort _EVRestarterConfigRepository;
        private readonly IInboundPortLocalizationProvider _localizationService;
        private readonly IRemoveApiCredentialsUseCase _removeApiCredentialsUseCase;
        private readonly AppConfig _currentConfig;

        public ViewModelOptionsApi(
            IDialogService dialogService,
            IEVisitorConfigRepositoryOutboundPort EVRestarterConfigRepository,
            IInboundPortLocalizationProvider LocalizationProvider,
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







