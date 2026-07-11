using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;
using eBRestarter.Core.Domain.ValueObjects;
using eBRestarter.Desktop.WinUI3.Enums;
using eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.SignalDTO.Messages;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public sealed partial class ViewModelOptionsApi : ObservableObject
    {
        private readonly IDialogService _dialogService;
        private readonly IOutboundPortEVisitorConfigRepository _EVRestarterConfigRepository;
        private readonly IInboundPortLocalizationProvider _localizationService;
        private readonly IUseCaseRemoveApiCredentials _removeApiCredentialsUseCase;
        private readonly AppConfig _currentConfig;

        public ViewModelOptionsApi(
            IDialogService dialogService,
            IOutboundPortEVisitorConfigRepository EVRestarterConfigRepository,
            IInboundPortLocalizationProvider LocalizationProvider,
            IUseCaseRemoveApiCredentials removeApiCredentialsUseCase)
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







