using eBRestarter.Classes.EBFiles;
using eBRestarter.Classes.EBInterfaces;
using eBRestarter.Classes.EBTask;
using eBRestarter.Classes.Services;
using eBRestarter.Services;
using eBRestarter.Themes;
using eBRestarter.Views.UserControls;
using eBRestarter.Views.Windows;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using eBRestarter.ViewModel.ViewModels;
using eBRestarter.Classes.Cache;

namespace eBRestarter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly EBTask _eBTask = new();

        public App()
        {
            ServiceCollection _ServiceCollection = new();
            ConfigureServices(_ServiceCollection);
            _serviceProvider = _ServiceCollection.BuildServiceProvider();
            Log.Logger = new LoggerConfiguration().WriteTo.File(ISystemPaths.File_Path_Logging).CreateLogger();
        }

        private void InitializeApplication()
        {
            System.Threading.Thread.Sleep(3000);
        }

        protected async override void OnStartup(StartupEventArgs e)
        {
            string procName = Process.GetCurrentProcess().ProcessName;
     
            Process[] processes = Process.GetProcessesByName(procName);

            if (processes.Length > 1)
            {
                MessageBox.Show("Der eB Restarter läuft bereits.",
                                "Hinweis",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                Application.Current.Shutdown();
            }
            else
            {

                // Show the SplashScreen
                var splashScreen = new SplashScreenWindow();
                splashScreen.Show();

                await Task.Run(InitializeApplication);

                IEBXMLFile eBXMLFileManagement = new EBXMLFileManagement();
                EBXMLFileService eBXMLFileService = new(eBXMLFileManagement);

                IEBFile eBFileManagement = new EBFileManagement();
                EBFileService eBFileService = new(eBFileManagement);

                var mainView = _serviceProvider.GetService<EBRestarterMainWindow>();

                if (mainView is null)
                {
                    MessageBox.Show("Problem im ServiceProvider");
                    Current.Shutdown();
                }

                ViewManager.InitViewManagerData(mainView!, _serviceProvider);

                eBFileService.CreateFile(ISystemPaths.File_Path_Time_Stamp_Delete_Process, "false"); //Wenn schon erzeugt, wird nicht nochmal erstellt
                eBFileService.CreateFile(ISystemPaths.File_Path_Time_Stamp_Restart_Process, "false"); //Wenn schon erzeugt, wird nicht nochmal erstellt

                SettingsCache.Position.Add("RestartDate", eBFileService.ReadFile(ISystemPaths.File_Path_Time_Stamp_Restart_Process));
                SettingsCache.Position.Add("RestartTime", eBXMLFileService.GetXMLTagValueInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.RestartTimeTag));
                SettingsCache.Position.Add("RestartComputer", eBXMLFileService.GetXMLTagValueInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.RestartComputerTag));

                string[]? credentials = eBFileService.ReadAPIFile();

                if (credentials is not null)
                {
                    SettingsCache.APIPosition.Add("APIUsername", credentials![0]);
                    SettingsCache.APIPosition.Add("APIKey", credentials![1]);
                }
                else
                {
                    SettingsCache.APIPosition.Add("APIUsername", null!);
                    SettingsCache.APIPosition.Add("APIKey", null!);
                }

                if (eBXMLFileService.GetXMLTagValueInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.ThemeTag).Equals("Light"))
                {
                    ThemeManager.ApplyLightMode();
                }
                else if (eBXMLFileService.GetXMLTagValueInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.ThemeTag).Equals("Dark"))
                {
                    ThemeManager.ApplyDarkMode();
                }

                //Important: If there is still a date in the restart file and a false in the XML Config, then the restart file must be set to false.
                if (eBXMLFileService.GetXMLTagValueInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.DeleteCookiesAndCacheTag).Equals("false"))
                {
                    eBFileService.WriteFile(ISystemPaths.File_Path_Time_Stamp_Delete_Process, "false");

                }
                else if ((eBFileService.ReadFile(ISystemPaths.File_Path_Time_Stamp_Delete_Process).Equals("false") &&
                          eBXMLFileService.GetXMLTagValueInConfig<int>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.DeleteCookiesAndCacheTag) >= 1) is true)
                {
                    eBXMLFileService.WriteXMLTagValueInConfig(IConfigConstants.DeleteCookiesAndCacheTag, "false");
                }

                if (eBXMLFileService.GetXMLTagValueInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.RestartComputerTag).Equals("false"))
                {
                    eBFileService.WriteFile(ISystemPaths.File_Path_Time_Stamp_Restart_Process, "false");

                }
                else if ((eBFileService.ReadFile(ISystemPaths.File_Path_Time_Stamp_Restart_Process).Equals("false") &&
                          eBXMLFileService.GetXMLTagValueInConfig<int>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.RestartComputerTag) >= 1) is true)
                {
                    eBXMLFileService.WriteXMLTagValueInConfig(IConfigConstants.RestartComputerTag, "false");
                }

                DateStampService dateStampService = new();
                dateStampService.DateIsLessThanTodayForDeletionProcess();
                dateStampService.DateIsLessThanTodayForRestartProcess();

                EBFileAccess.CreateFileLock!(IFileNames.EBRESTARTERCONFIG, ISystemPaths.File_Path_eBRestarter_Settings);
                EBFileAccess.CreateFileLock!(IFileNames.DELETEDATE, ISystemPaths.File_Path_Time_Stamp_Delete_Process);
                EBFileAccess.CreateFileLock!(IFileNames.RESTARTDATE, ISystemPaths.File_Path_Time_Stamp_Restart_Process);

                EBFileAccess.AccessFile!(IFileNames.DELETEDATE);
                RestartBrowserTaskMediatorViewModel.BrowserContentDeleteDate = eBFileService.ReadFile(ISystemPaths.File_Path_Time_Stamp_Delete_Process);
                EBFileAccess.LockFile!(IFileNames.DELETEDATE);

                _eBTask.RestartClient();

                PreloadViews();

                splashScreen.Close();

                mainView!.Show();

                base.OnStartup(e);

            }

            
        }

        private void PreloadViews()
        {
            ViewManager.ShowPageOnMainView<UC_InfocenterView>();
            ViewManager.ShowPageOnMainView<UC_OptionsView>();
            ViewManager.ShowPageOnMainView<UC_MainSettingsView>(); //true = fullscreen
            ViewManager.ShowPageOnMainView<UC_DashboardView>();
        }

        private void ConfigureServices(ServiceCollection iServiceCollection)
        {
            //Here all services are collected, and says how it should take the services.
            //Since it is a singleton, the stored variable values remain in the page calls.

            iServiceCollection.AddSingleton<EBRestarterMainWindow>(); //MainView is always displayed at startup, is only called up once, later contains all our views
            iServiceCollection.AddSingleton<EBRestarterMainViewModel>();

            iServiceCollection.AddSingleton<UC_DashboardView>();
            //iServiceCollection.AddSingleton<DashboardViewModel>();

            iServiceCollection.AddSingleton<UC_MainSettingsView>();

            iServiceCollection.AddSingleton<UC_InfocenterView>();
            iServiceCollection.AddSingleton<InfocenterViewModel>();


            iServiceCollection.AddSingleton<UC_OptionsView>();
            iServiceCollection.AddSingleton<OptionsViewModel>();

            //With AddTransient, the pages are called up again and the object is recreated
            //iServiceCollection.AddTransient<MainView>();

        }


    }
}
