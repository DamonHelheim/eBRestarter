using MahApps.Metro.Controls;
using System;
using eBRestarter.ViewModel.ViewModels;
using eBRestarter.Classes.Cache;
using eBRestarter.Classes.EBEvents;

namespace eBRestarter.Views.Windows
{

    public partial class ActivateAPIAccess : MetroWindow
    {
        private readonly UserNameEnteredEvent? userNameEnteredEvent;
        private ActivateAPIAccessViewModel ActivateAPIAccessViewModel { get; } = new();

        public ActivateAPIAccess(UserNameEnteredEvent nameEnteredEvent)
        {
            InitializeComponent();

            this.DataContext = ActivateAPIAccessViewModel;
            this.userNameEnteredEvent = nameEnteredEvent;
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {          
            if (SettingsCache.APIPosition["APIUsername"] is not null && SettingsCache.APIPosition["APIKey"] is not null)
            {
                userNameEnteredEvent!.RaiseNameEnteredEvent("Nutzername: " + SettingsCache.APIPosition["APIUsername"]);
            }
            else
            {
                userNameEnteredEvent!.RaiseNameEnteredEvent("Nutzername: ");
            }          
        }
    }
}
