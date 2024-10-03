using eBRestarter.Classes.EBInterfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBRestarter.Classes.OperatingSystem;
using eBRestarter.Classes.EBFiles;
using eBRestarter.Classes.InternetBrowser;
using eBRestarter.Classes.Services;


namespace eBRestarter.ViewModel.ViewModels
{
    public partial class EVisitorUsernameViewModel : BaseViewModel
    {

        [ObservableProperty]
        public string username = string.Empty;

        private readonly Browser chromeBrowser = new Chrome(IWebLinks.REGISTRATION_LINK);
        private readonly Browser firefoxBrowser = new Firefox(IWebLinks.REGISTRATION_LINK);
        private readonly Browser edgeBrowser = new Edge(IWebLinks.REGISTRATION_LINK);

        private readonly IEBXMLFile eBXMLFileManagement = new EBXMLFileManagement();

        public EVisitorUsernameViewModel() { 
        
        }

        [RelayCommand]
        private void RegisterToEVisitor()
        {
            WindowsOS windowsOS = new();

            windowsOS.OpenLinkWithStandardbrowser(IWebLinks.REGISTRATION_LINK);         
        }


        [RelayCommand(CanExecute = nameof(CanExecuteAddUsername))]
        public void ExecuteAddUsername(object addUsername)
        {
            EBXMLFileService eBFileService = new(eBXMLFileManagement);

            eBFileService.WriteXMLAttributeInConfig(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.UsernameTag, IConfigConstants.UsernameTag_Name_Attribut, (addUsername as string)!);

            RestartBrowserTaskMediatorViewModel.UsernameMediator = eBFileService.GetXMLAttributeInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.UsernameTag, IConfigConstants.UsernameTag_Name_Attribut);

            RestartBrowserTaskMediatorViewModel.TriggerUsernameMediator = true;

            Username = "";
        }

      
        public bool CanExecuteAddUsername(object value)
        {
            if (string.IsNullOrEmpty(Username))
            {
                return false;

            } else { return true; }           
        }

    }
}
