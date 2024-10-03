using eBRestarter.Classes.EBInterfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using eBRestarter.Classes.Services;
using System.Collections.ObjectModel;
using eBRestarter.Classes.EBFiles;

namespace eBRestarter.ViewModel.ViewModels
{
    public partial class AutomatedTasksViewModel : BaseViewModel
    {
        [ObservableProperty]
        private bool tSStartBrowserWithProgrammStartIsOn = false;

        [ObservableProperty]
        private bool tSCheckBrowserIsAliveIsOn = false;

        [ObservableProperty]
        private int comboboxSelectedIndex;

        [ObservableProperty]
        private ObservableCollection<string> listComboBoxDeleteBrowserCache =
        [
           "Cache und Cookies nicht löschen",
           "Cache und Cookies jeden Tag löschen",
           "Cache und Cookies alle 3 Tage löschen",
           "Cache und Cookies alle 7 Tage löschen",
           "Cache und Cookies alle 14 Tage löschen"
        ];

        private readonly IEBXMLFile eBXMLFileManagement = new EBXMLFileManagement();
        private readonly IEBFile eBFileManagement = new EBFileManagement();

        public AutomatedTasksViewModel() {

            EBXMLFileService eBXMLFileService = new(eBXMLFileManagement);

            TSStartBrowserWithProgrammStartIsOn = eBXMLFileService.GetXMLTagValueInConfig<bool>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.StartRestarterWithProgrammStartTag);

            TSCheckBrowserIsAliveIsOn = eBXMLFileService.GetXMLTagValueInConfig<bool>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.CheckBrowserAliveRoutineTag);

            if (eBXMLFileService.GetXMLTagValueInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.DeleteCookiesAndCacheTag).Equals("false"))
            {
                ComboboxSelectedIndex = 0;
            }
            else
            {
                int IndexTrigger = eBXMLFileService.GetXMLTagValueInConfig<int>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.DeleteCookiesAndCacheTag);

                switch (IndexTrigger)
                {
                    case 1: ComboboxSelectedIndex = 1; break;
                    case 3: ComboboxSelectedIndex = 2; break;
                    case 7: ComboboxSelectedIndex = 3; break;
                    case 14: ComboboxSelectedIndex = 4; break;
                }
            }
        }

        
        public void SetDeleteDate(int daysInFuture)
        {
            DateStampService daysFutureObject = new();
            string a = "";

            EBFileService eBFileService = new(eBFileManagement);
            EBFileAccess.AccessFile!(IFileNames.DELETEDATE);

            if (daysInFuture is 0)
            {   
                eBFileService.WriteFile(ISystemPaths.File_Path_Time_Stamp_Delete_Process, "false");
                a = eBFileService.ReadFile(ISystemPaths.File_Path_Time_Stamp_Delete_Process);
                
            } else
            {
                eBFileService.WriteFile(ISystemPaths.File_Path_Time_Stamp_Delete_Process, DateStampService.GetFutureDate(daysInFuture));
                a = eBFileService.ReadFile(ISystemPaths.File_Path_Time_Stamp_Delete_Process);              
            }

            RestartBrowserTaskMediatorViewModel.BrowserContentDeleteDate = a;

            EBFileAccess.LockFile!(IFileNames.DELETEDATE);
        }

        public void SaveDeleteBrowserCacheValue(string value)
        {
            EBXMLFileService eBFileService = new(eBXMLFileManagement);

            eBFileService.WriteXMLTagValueInConfig(IConfigConstants.DeleteCookiesAndCacheTag, value);
        }


        public void SetStartRestarterWithProgrammStart(bool a)
        {
            EBXMLFileService eBFileService = new(eBXMLFileManagement);

            eBFileService.WriteXMLTagValueInConfig(IConfigConstants.StartRestarterWithProgrammStartTag, a.ToString());
        }

        public void SetCheckBrowserAliveRoutine(bool a)
        {
            EBXMLFileService eBFileService = new(eBXMLFileManagement);

            eBFileService.WriteXMLTagValueInConfig(IConfigConstants.CheckBrowserAliveRoutineTag, a.ToString());
        }
    }
}
