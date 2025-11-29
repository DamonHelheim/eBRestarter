using CommunityToolkit.Mvvm.Input;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using eBRestarter.Desktop.WinUI3.Views.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public class MainViewModel
    {
        private readonly INavigationService _navigationService;

        public ICommand OpenOptionsCommand { get; }

        public MainViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            OpenOptionsCommand = new RelayCommand(() => _navigationService.Navigate<P_CommonOverview>());
        }
    }

}
