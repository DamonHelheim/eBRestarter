using eBRestarter.Classes.Services;
using MahApps.Metro.Controls;
using System.Windows;
using eBRestarter.Classes.EBInterfaces;
using eBRestarter.Themes;
using eBRestarter.Classes.EBFiles;

namespace eBRestarter.Views.Windows
{

    public partial class SplashScreenWindow : MetroWindow
    {       
        public SplashScreenWindow()
        {
            InitializeComponent();

            IEBFile eBFileManagement = new EBFileManagement();
            EBFileService eBFileService = new(eBFileManagement);
            eBFileService.CreateDefaultDirectoryIfNotExist();

            IEBXMLFile eBXMLFileManagement = new EBXMLFileManagement();
            EBXMLFileService eBXMLFileService = new(eBXMLFileManagement);

            eBXMLFileService.CreateDefaultConfigFileIfNotExist();

            ConsistancyCheck.CheckFileConsistency();

            if (eBXMLFileService.GetXMLTagValueInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.ThemeTag).Equals("Light"))
            {
                ThemeManager.ApplyLightMode();
            }
            else if (eBXMLFileService.GetXMLTagValueInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.ThemeTag).Equals("Dark"))
            {
                ThemeManager.ApplyDarkMode();
            }

            // Remove the window frame options
            this.WindowStyle = WindowStyle.None;

            // Deactivate changing the window size
            this.ResizeMode = ResizeMode.NoResize;
        }
    }
}
