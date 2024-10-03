using eBRestarter.Classes.Services;
using eBRestarter.Views.Windows;
using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using eBRestarter.ViewModel.ViewModels;
using eBRestarter.Classes.EBInterfaces;
using eBRestarter.Classes.EBFiles;
using System.IO;
using eBRestarter.Themes;
using eBRestarter.Services;
using eBRestarter.Classes.Cache;
using eBRestarter.Classes.EBEvents;


namespace eBRestarter.Views.UserControls
{

    public partial class UC_OptionsView : UserControl
    {
        private readonly bool _valueChanged = false;

        private readonly IEBXMLFile eBXMLFileManagement = new EBXMLFileManagement();
        private readonly IEBFile eBFileManagement = new EBFileManagement();

        private readonly UserNameEnteredEvent? userNameEnteredEvent;

        public OptionsViewModel OptionsViewModel { get; } = new();

        public UC_OptionsView(OptionsViewModel optionsViewModel)
        {
            OptionsViewModel = optionsViewModel;
            DataContext = OptionsViewModel;
            
            InitializeComponent();
            InitializeTrackSavedSettings();
 
            EBXMLFileService eBXMLFileService = new(eBXMLFileManagement);
           
            if (SettingsCache.APIPosition["APIUsername"] is not null && SettingsCache.APIPosition["APIKey"] is not null)
            {
                lbl_APIUsername.Text = "Nutzername: " + SettingsCache.APIPosition["APIUsername"];
            }
            else
            {
                lbl_APIUsername.Text = "Nutzername: -";
            }

            _valueChanged = true;

            userNameEnteredEvent = new UserNameEnteredEvent();
            userNameEnteredEvent.UserNameEntered += OnNameEntered!;

            SettingsCache.Position[IConfigConstants.RestartTimeTag] = eBXMLFileService.GetXMLTagValueInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.RestartTimeTag).ToString();
            SettingsCache.Position[IConfigConstants.RestartComputerTag] = eBXMLFileService.GetXMLTagValueInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.RestartComputerTag).ToString();

        }

        private void OnNameEntered(object sender, NameEnteredEventArgs e)
        {
            lbl_APIUsername.Text = e.UserName;
        }

        private void BtnConnectWithAPIInterface_Click(object sender, RoutedEventArgs e)
        {
            ActivateAPIAccess activateAPIAccess = new(userNameEnteredEvent!);
            activateAPIAccess.ShowDialog();
        }
        private void BtnImportAPIFile_Click(object sender, RoutedEventArgs e)
        {
            ImportAPIFile importAPIFile = new(userNameEnteredEvent);
            importAPIFile.ShowDialog();
        }

        private void BtnOptionsAutomaticWindowsUserLogin_Click(object sender, RoutedEventArgs e)
        {
            OpenAutomaticWindowsUserLogin();
        }

        private void ComboboxComputerRestartOptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_valueChanged == true)
            {
                LoadSliderSettings();
            }
        }


        private void ComputerRestartOption(bool sliderIsEnabled, string a, int b)
        {
            OptionsViewModel!.SaveRestartComputerOptionsValue(a);
            OptionsViewModel!.SetRestartDate(b);

            SettingsCache.Position["RestartTime"] = a;
            SettingsCache.Position["RestartComputer"] = b.ToString();

            if (sliderIsEnabled == true)
            {
                SliderRestartComputer.IsEnabled = sliderIsEnabled;
                SliderRestartComputer.IsEnabled = sliderIsEnabled;
                tb_RestartComputerInput.IsEnabled = sliderIsEnabled;

                EBXMLFileService eBXMLFileService = new(eBXMLFileManagement);

                SliderRestartComputer.Value = eBXMLFileService.GetXMLTagValueInConfig<int>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.RestartTimeTag);

                lbl_Options_RestartTime.Text = "um " + SliderRestartComputer.Value.ToString() + ":00 Uhr neugestartet.";
            }
            else
            {
                    SliderRestartComputer.IsEnabled = sliderIsEnabled;
                    SliderRestartComputer.IsEnabled = sliderIsEnabled;
                    tb_RestartComputerInput.IsEnabled = sliderIsEnabled;
                    lbl_Options_DateOfRestartComputerInfo.Text = "Computer wird nicht neugestartet.";
                    lbl_Options_RestartTime.Text = "";
            }

        }

        private void SliderRestartComputer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SliderRestartComputer_SetValues();
        }

        private void SliderRestartComputer_SetValues()
        {
            if (_valueChanged == true)
            {
                lbl_Options_RestartTime.Text = "um " + SliderRestartComputer.Value.ToString() + ":00 Uhr neugestartet.";

                EBXMLFileService eBXMLFileService = new(eBXMLFileManagement);
                eBXMLFileService.WriteXMLTagValueInConfig("RestartTime", SliderRestartComputer.Value.ToString());

                SettingsCache.Position["RestartTime"] = eBXMLFileService.GetXMLTagValueInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.RestartTimeTag);
            }
        }

        private void LoadSliderSettings()
        {
            string riskInformation = "Nutzung auf eigenes Risiko: Du erklärst dich mit der Nutzung dieser Funktionalität einverstanden. Für mehr Infos hier klicken";

            if (ComboboxComputerRestartOptions.SelectedIndex is 0)
            {
                tbl_restart_information.Text = "";
                ComputerRestartOption(false, "false", 0);
            }
            else if (ComboboxComputerRestartOptions.SelectedIndex is 1)
            {
                tbl_restart_information.Text = riskInformation;
                ComputerRestartOption(true, "1", 1);
            }
            else if (ComboboxComputerRestartOptions.SelectedIndex is 2)
            {
                tbl_restart_information.Text = riskInformation;
                ComputerRestartOption(true, "3", 3);
            }
            else if (ComboboxComputerRestartOptions.SelectedIndex is 3)
            {
                tbl_restart_information.Text = riskInformation;
                ComputerRestartOption(true, "7", 7);
            }
            else if (ComboboxComputerRestartOptions.SelectedIndex is 4)
            {
                tbl_restart_information.Text = riskInformation;
                ComputerRestartOption(true, "14", 14);
            }
        }

        private void InitializeTrackSavedSettings()
        {
            LoadSliderSettings();

            EBXMLFileService eBXMLFileService = new(eBXMLFileManagement);

            string a = eBXMLFileService.GetXMLTagValueInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.RestartComputerTag);

            if (a.Equals("false"))
            {
                SliderRestartComputer.Value                 = eBXMLFileService.GetXMLTagValueInConfig<int>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.RestartTimeTag);
                lbl_Options_DateOfRestartComputerInfo.Text  = "Computer wird nicht neugestartet.";
                lbl_Options_RestartTime.Text                = "";
            }
            else
            {
                SliderRestartComputer.Value                 = eBXMLFileService.GetXMLTagValueInConfig<int>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.RestartTimeTag);
                lbl_Options_RestartTime.Text                = "um " + SliderRestartComputer.Value.ToString() + ":00 Uhr neugestartet.";
            }
        }

        private void OnKeyDownHandlerSliderRestartComputerTime(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (int.Parse(tb_RestartComputerInput.Text) > 24)
                {
                    SliderRestartComputer.Value = 23;
                    tb_RestartComputerInput.Text = 23.ToString();
                    SliderRestartComputer_SetValues();
                }
                else if (int.Parse(tb_RestartComputerInput.Text) < 1)
                {
                    SliderRestartComputer.Value = 1;
                    tb_RestartComputerInput.Text = 1.ToString();
                    SliderRestartComputer_SetValues();
                }
                else
                {
                    SliderRestartComputer.Value = int.Parse(tb_RestartComputerInput.Text);
                    SliderRestartComputer_SetValues();
                }               
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);

            AppDomain.CurrentDomain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", TimeSpan.FromMilliseconds(100));
        }

        private void OpenAutomaticWindowsUserLogin()
        {
            if (!UC_OptionsView.IsAdministrator())
            {
                string message = "Du musst dieses Programm als Administrator starten, damit du die automatische Anmeldung bei dir aktivieren kannst. Das Programm muss dazu neugestartet werden." + "\r\n" + "\r\n" +
                                 "Möchtest Du das Programm jetzt als Administrator starten?";

                string title = "Programm als Administrator starten";

                MessageBoxResult result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    string? exeName = Environment.ProcessPath;

                    ProcessStartInfo startInfo = new(exeName!)
                    {
                        UseShellExecute = true,
                        Verb = "runas",
                    };

                    Process.Start(startInfo);
                    Application.Current.Shutdown();
                }
            }
            else
            {
                _ = new AutomaticUserLogInGuidlineWindows().ShowDialog();
            }
        }


        public static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void Btn_go_to_settings_data_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", ISystemPaths.FilePathApplicationOnly);
        }
     
        private void BtnGoToSavedAPIFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "EB_API_File", // Default file name
                DefaultExt = ".apiaf", // Default file extension
                Filter = "eB API File (*.apiaf)|*.apiaf", // Filter files by extension
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                RestoreDirectory = true
            };

            string apiFileName = @"\EB_API_File.apiaf";
            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                _ = dlg.FileName;

                string? folderName = Path.GetDirectoryName(dlg.FileName + dlg.DefaultExt);

                File.Copy(ISystemPaths.File_Path_API, folderName + apiFileName, true);

                File.SetAttributes(folderName + apiFileName, FileAttributes.Normal);

                MessageBox.Show("API Datei wurde erfolgreich exportiert!", "Datei Export", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
       
        private void BtnRemoveAPIFile_Click(object sender, RoutedEventArgs e)
        {
            IEBFile eBFileManagement = new EBFileManagement();
            EBFileService eBFileService = new(eBFileManagement);

            string message = "Willst Du die API Schnittstelle wirklich deaktivieren?" + "\n" + "Die entfernte API-Datei wird separat gesichert.";
            string title = "Entferne Lizenz";

            MessageBoxResult result2 = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Information);

            if (result2 == MessageBoxResult.Yes)
            {
                eBFileService.RenameFileAndSaveInBackup(ISystemPaths.FilePathApplicationOnly, "EB_API_File", ".apiaf", "APIFileOld") ;

                SettingsCache.APIPosition["APIUsername"] = null!;
                SettingsCache.APIPosition["APIKey"] = null!;

                lbl_APIUsername.Text = "Nutzername: -";

                OptionsViewModel.BtnAPISectionIsEnabled = false;
            }
        }

        private void BtnShowAPIKey_Click(object sender, RoutedEventArgs e)
        {
            string title = "API Schlüssel";

            if (SettingsCache.APIPosition["APIUsername"] is not null && SettingsCache.APIPosition["APIKey"] is not null)
            {
                MessageBox.Show(SettingsCache.APIPosition["APIKey"], title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Kein API Key gefunden", title, MessageBoxButton.OK, MessageBoxImage.Information);
            }          
        }

        
        private void BtnSetDarkTheme_Click(object sender, RoutedEventArgs e)
        {
            //Application.Current.Resources.MergedDictionaries.Clear();
            EBXMLFileService eBFileService = new(eBXMLFileManagement);
            eBFileService.WriteXMLTagValueInConfig(IConfigConstants.ThemeTag, "Dark");

            ThemeManager.ApplyDarkMode();
        }

        private void BtnSetStandardTheme_Click(object sender, RoutedEventArgs e)
        {
            //Application.Current.Resources.MergedDictionaries.Clear();
            EBXMLFileService eBFileService = new(eBXMLFileManagement);
            eBFileService.WriteXMLTagValueInConfig(IConfigConstants.ThemeTag, "Light");

            ThemeManager.ApplyLightMode();
        }

        private void BtnCheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            UpdaterHelper.ManualUpdate();
        }

        private void Tbl_restart_information_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Durch die Nutzung dieser Funktion des automatischen Neustarts, erklärst du dich damit einverstanden, " +
                            "dass du die Risiken verstanden hast und die Nutzung dieser Funktionalität auf eigene Verantwortung erfolgt." + '\n' + '\n' +
                            "Der Entwickler übernimmt keine Haftung für mögliche Schäden, Datenverlust oder Systemfehler, die durch" + '\n' +
                            "den erzwungenen Neustart entstehen können",
                            "Nutzung auf eigenes Risiko",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }
    }
}
