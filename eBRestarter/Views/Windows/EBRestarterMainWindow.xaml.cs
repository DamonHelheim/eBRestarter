using eBRestarter.Services;
using eBRestarter.Views.UserControls;
using MahApps.Metro.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using eBRestarter.ViewModel.ViewModels;

namespace eBRestarter.Views.Windows
{

    public partial class EBRestarterMainWindow : MetroWindow
    {
        public EBRestarterMainViewModel? EBRestarterMainViewModel { get; } = new();

        public EBRestarterMainWindow(EBRestarterMainViewModel eBRestarterMainViewModel)
        {
            InitializeComponent();

            EBRestarterMainViewModel = eBRestarterMainViewModel;

            DataContext = EBRestarterMainViewModel;

            this.Title = "Version: " + VersionHelper.GetCurrentVersion!;

            CheckForUpdate();

            CheckForUpdateFrequentlyTask();
        }

        public void CheckForUpdateFrequentlyTask()
        {
            DispatcherTimer timer = new() { Interval = TimeSpan.FromMinutes(1440) };

            timer.Tick += delegate
            {
                CheckForUpdate();
            };

            timer.Start();

        }

        private void CheckForUpdate()
        {
            if (UpdaterHelper.NewVersionAvailable() == true)
            {
                this.BeginInvoke(new Action(() => {

                    UserPanel.Visibility = Visibility.Visible;

                }));
            }
            else
            {
                this.BeginInvoke(new Action(() => {

                    UserPanel.Visibility = Visibility.Hidden;

                }));
            }
        }


        private void OpenPageOnMain(object sender, RoutedEventArgs e)
        {
            if (sender is Button Btn)
            {
                if (Btn == BtnDashboard)
                {
                    ViewManager.ShowPageOnMainView<UC_DashboardView>();
                }
                else if (Btn == BtnSettings)
                {
                    ViewManager.ShowPageOnMainView<UC_MainSettingsView>();
                }
                else if (Btn == BtnOptions)
                {
                    ViewManager.ShowPageOnMainView<UC_OptionsView>();
                }
                else if (Btn == BtnInfocenter)
                {
                    ViewManager.ShowPageOnMainView<UC_InfocenterView>();
                }
            }
        }

        private void MenuBtnLogOut_Click(object sender, RoutedEventArgs e)
        {
            UpdaterHelper.ManualUpdate();
        }
    }
}
