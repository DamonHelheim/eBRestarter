using eBRestarter.Classes.EBInterfaces;
using eBRestarter.Classes.OperatingSystem;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using eBRestarter.Classes.Services;
using System.Collections.ObjectModel;
using eBRestarter.Classes.EBFiles;
using eBRestarter.Classes.Cache;


namespace eBRestarter.ViewModel.ViewModels
{
    public partial class OptionsViewModel : BaseViewModel
    {
        #pragma warning disable CA1416
        private readonly WindowsOS windowsOS = new();

        private readonly IEBXMLFile eBXMLFileManagement = new EBXMLFileManagement();
        private readonly IEBFile    eBFileManagement    = new EBFileManagement();

        public const int Restart_Clock_Time_MIN = 1;
        public const int Restart_Clock_Time_MAX = 23;

        [ObservableProperty]
        public bool setStartUp;

        [ObservableProperty]
        public int comboboxRestartComputerSelectedIndex;

        [ObservableProperty]
        public string? options_DateOfRestartComputerInfo = string.Empty;

        [ObservableProperty]
        public bool? btnAPISectionIsEnabled;

        [ObservableProperty]
        public ObservableCollection<string> listComboBoxRestartComputerOptions =
        [
                "Computer nicht neustarten",
                "Computer jeden Tag neustarten",
                "Computer alle 3 Tage neustarten",
                "Computer alle 7 Tage neustarten",
                "Computer alle 14 Tage neustarten"
        ];

        public OptionsViewModel() {

            EBXMLFileService eBXMLFileService = new(eBXMLFileManagement);

            WindowsOS windowsOS = new();

            string containsValue = string.Empty;

            if (eBXMLFileService.GetXMLTagValueInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.StartWithWindowsTag).Equals("true"))
            {
                foreach (var item in windowsOS.GetRegistrySubKeysStartUp())
                {
                    if (item.Key.Equals("eB Restarter"))
                    {
                        containsValue = item.Key;

                        break;
                    }
                }

                if (!containsValue.Equals("eB Restarter"))
                {
                    windowsOS.SetStartUp();
                }
            }
            else if (eBXMLFileService.GetXMLTagValueInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.StartWithWindowsTag).Equals("false"))
            {
                foreach (var item in windowsOS.GetRegistrySubKeysStartUp())
                {
                    if (item.Key.Equals("eB Restarter"))
                    {
                        containsValue = item.Key;

                        break;
                    }
                }

                if (containsValue.Equals("eB Restarter"))
                {
                    eBXMLFileService.WriteXMLTagValueInConfig("StartWithWindows", "true");
                }
            }

            SetStartUp = eBXMLFileService.GetXMLTagValueInConfig<bool>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.StartWithWindowsTag);

            if (eBXMLFileService.GetXMLTagValueInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.RestartComputerTag).Equals("false"))
            {
                ComboboxRestartComputerSelectedIndex = 0;
            }
            else
            {
                int IndexTrigger = eBXMLFileService.GetXMLTagValueInConfig<int>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.RestartComputerTag);

                switch (IndexTrigger)
                {
                    case 1: ComboboxRestartComputerSelectedIndex  = 1; break;
                    case 3: ComboboxRestartComputerSelectedIndex  = 2; break;
                    case 7: ComboboxRestartComputerSelectedIndex  = 3; break;
                    case 14: ComboboxRestartComputerSelectedIndex = 4; break;
                }             
            }

            CheckButtonEnabled();
        }

        public void CheckButtonEnabled()
        {
            Task.Run( async () => {

                while (true)
                {

                    if (File.Exists(ISystemPaths.File_Path_API))
                    {
                        BtnAPISectionIsEnabled = true;
                    }
                    else
                    {
                        BtnAPISectionIsEnabled = false;
                    }

                    await Task.Delay(1000);
                }
            
            });
        }

        [RelayCommand]
        private void ExecuteCbxOptionsStartWithWindows(object value)
        {
            bool activateStartWithWindows = (bool)value;
            EBXMLFileService eBXMLFileService = new(eBXMLFileManagement);

            if (activateStartWithWindows is true)
            {
                eBXMLFileService.WriteXMLTagValueInConfig(IConfigConstants.StartWithWindowsTag, "true");
                windowsOS.SetStartUp();

            } else {

                eBXMLFileService.WriteXMLTagValueInConfig(IConfigConstants.StartWithWindowsTag, "false");
                windowsOS.DeleteStartUp();

            }
        }

        public void SaveRestartComputerOptionsValue(string value)
        {
            EBXMLFileService eBXMLFileService = new(eBXMLFileManagement);

            eBXMLFileService.WriteXMLTagValueInConfig("RestartComputer", value);
        }

        public void SetRestartDate(int daysInFuture)
        {
            DateStampService daysFutureObject = new();
            string timestamp = string.Empty;

            EBFileService eBFileService = new(eBFileManagement);
            EBFileAccess.AccessFile!(IFileNames.RESTARTDATE);

            if (daysInFuture is 0)
            {
                eBFileService.WriteFile(ISystemPaths.File_Path_Time_Stamp_Restart_Process, "false");
                Options_DateOfRestartComputerInfo = "Computer wird nicht neugestartet";
            }
            else
            {
                eBFileService.WriteFile(ISystemPaths.File_Path_Time_Stamp_Restart_Process, DateStampService.GetFutureDate(daysInFuture));

                timestamp = eBFileService.ReadFile(ISystemPaths.File_Path_Time_Stamp_Restart_Process);

                SettingsCache.Position["RestartDate"] = timestamp;

                Options_DateOfRestartComputerInfo = "Computer wird am " + timestamp;
            }

            EBFileAccess.LockFile!(IFileNames.RESTARTDATE);
        }
    }
}
