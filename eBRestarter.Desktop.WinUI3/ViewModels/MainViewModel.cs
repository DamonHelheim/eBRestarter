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
        // =========================================================
        // 1. FIELDS & INJECTED SERVICES (Backing-Felder und DI)
        // =========================================================
        #region FieldsAndInjectedServices

        private readonly INavigationService _navigationService;

        #endregion

        // =========================================================
        // 2. CONSTRUCTOR & FINALIZER (Ctor)
        // =========================================================
        #region ConstructorAndFinalizer

        public MainViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        #endregion
    }
}
