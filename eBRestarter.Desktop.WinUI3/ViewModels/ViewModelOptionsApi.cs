using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Domain.Entities;
using eBRestarter.Core.Application.UseCases.RemoveApiCredentials;
using eBRestarter.Desktop.WinUI3.Messages;
using eBRestarter.Desktop.WinUI3.Models.Enums;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public partial class ViewModelOptionsApi : ObservableObject
    {
        private readonly IDialogService _dialogService;
        private readonly IEVisitorConfigService _eVisitorConfigService;
        private readonly ILocalizationService _localizationService;
        private readonly IRemoveApiCredentialsUseCase _removeApiCredentialsUseCase;
        private readonly AppConfig _currentConfig;

        public ViewModelOptionsApi(
            IDialogService dialogService,
            IEVisitorConfigService eVisitorConfigService,
            ILocalizationService localizationService,
            IRemoveApiCredentialsUseCase removeApiCredentialsUseCase)
        {
            _dialogService = dialogService;
            _eVisitorConfigService = eVisitorConfigService;
            _localizationService = localizationService;
            _removeApiCredentialsUseCase = removeApiCredentialsUseCase;

            _currentConfig = _eVisitorConfigService.LoadConfig();
        }

        [RelayCommand]
        private async Task RemoveAPICredentials()
        {
            _removeApiCredentialsUseCase.Execute();

            _currentConfig.Settings.ApiUsername = string.Empty;
            _currentConfig.Settings.ApiKey = string.Empty;

            WeakReferenceMessenger.Default.Send(new ApiCredentialsRemovedMessage());

            await _dialogService.ShowMessageAsync(
                _localizationService.GetString("Options_RemoveCreds_Title"),
                _localizationService.GetString("Options_RemoveCreds_Message"),
                DialogIcon.Success);
        }

        [RelayCommand]
        private async Task ShowActivateApiDialog()
        {
            await _dialogService.ShowActivateApiDialogAsync();
        }
    }
}
