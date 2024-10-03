using eBRestarter.Classes.InternetBrowser;
using eBRestarter.Views.Windows;
using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Controls;
using eBRestarter.ViewModel.ViewModels;

namespace eBRestarter.Views.UserControls
{

    public partial class UC_AutomatedTasks : UserControl
    {
        private readonly Browser edgeBrowser = new Edge();
        private AutomatedTasksViewModel? AutomatedTasksViewModel { get; set; } = new();

        private readonly bool _valueChanged = false;

        public UC_AutomatedTasks()
        {
            DataContext = AutomatedTasksViewModel;           
            InitializeComponent();

            _valueChanged = true;
        }

        private void TSStartBrowserWithProgrammStart_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch? toggleSwitch = sender as ToggleSwitch;

            if (toggleSwitch != null)
            {
                if (toggleSwitch.IsOn == true)
                {
                    AutomatedTasksViewModel!.SetStartRestarterWithProgrammStart(true);
                }
                else
                {
                    AutomatedTasksViewModel!.SetStartRestarterWithProgrammStart(false);
                }
            }
        }

        private void TSCheckBrowserIsAlive_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch? toggleSwitch = sender as ToggleSwitch;

            if (toggleSwitch != null)
            {
                if (toggleSwitch.IsOn == true)
                {
                    AutomatedTasksViewModel!.SetCheckBrowserAliveRoutine(true);
                }
                else
                {
                    AutomatedTasksViewModel!.SetCheckBrowserAliveRoutine(false);
                }
            }
        }

        private void ComboboxDeleteBrowserCache_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_valueChanged == true)
            {

                if (ComboboxDeleteBrowserCache.SelectedIndex == 0)
                {
                    AutomatedTasksViewModel!.SaveDeleteBrowserCacheValue("false");
                    AutomatedTasksViewModel!.SetDeleteDate(0);
                }
                else if (ComboboxDeleteBrowserCache.SelectedIndex == 1)
                {
                    AutomatedTasksViewModel!.SaveDeleteBrowserCacheValue("1");
                    AutomatedTasksViewModel!.SetDeleteDate(1);
                }
                else if (ComboboxDeleteBrowserCache.SelectedIndex == 2)
                {
                    AutomatedTasksViewModel!.SaveDeleteBrowserCacheValue("3");
                    AutomatedTasksViewModel!.SetDeleteDate(3);                 
                }
                else if (ComboboxDeleteBrowserCache.SelectedIndex == 3)
                {
                    AutomatedTasksViewModel!.SetDeleteDate(7);
                    AutomatedTasksViewModel!.SaveDeleteBrowserCacheValue("7");

                }
                else if (ComboboxDeleteBrowserCache.SelectedIndex == 4)
                {
                    AutomatedTasksViewModel!.SetDeleteDate(14);
                    AutomatedTasksViewModel!.SaveDeleteBrowserCacheValue("14");
                }
            }
        }

        private void Tbl_info_startup_boost_edge_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            if (edgeBrowser.BrowserVersion != null && edgeBrowser.BrowserExist is true)
            {
                _ = new PopUpOpenEdgeBrowserWithLink().ShowDialog();
            }
            else
            {
                string message = "Der Edgebrwoser ist nicht installiert.";
                string title = "Edgebrowser";

                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CopyText_Click(object sender, RoutedEventArgs e)
        {          

            if (edgeBrowser.BrowserVersion != null && edgeBrowser.BrowserExist is true)
            {
                Clipboard.SetText("edge://settings/?search=Startup-Boost");
                MessageBox.Show("Folgender Link wurde jetzt kopiert: edge://settings/?search=Startup-Boost");

                string message = "Folgender Link wurde jetzt kopiert: edge://settings/?search=Startup-Boost" + '\n' + "Sobald du auf Ok klickst, öffnet sich der Edgebrowser und du kannst den Link zu einfügen.";
                string title = "Edgebrowser";
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);


                edgeBrowser.StartBrowser();

            } else
            {
                string message = "Der Edgebrwoser ist nicht installiert.";
                string title = "Edgebrowser"; 

                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);              
            }

        }
    }
}
