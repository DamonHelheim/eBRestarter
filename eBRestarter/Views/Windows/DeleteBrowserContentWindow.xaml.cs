using eBRestarter.Classes.EBFiles;
using eBRestarter.Classes.OperatingSystem;
using eBRestarter.Classes.Services;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace eBRestarter.Views.Windows
{

    public partial class DeleteBrowserContentWindow : MetroWindow
    {

        private int _progressbarValueIncrementAutomatic = 0;
        private int _progressbarValueLimitAutomatic = 0;

        private bool _stopDeleteProcess;
        private readonly bool _startAutomaticDeletion;

        private readonly string? _browserName;
       
        private readonly Delete delete = new();

        private List<string> choosenPathsList = [];

        private readonly Dictionary<string, List<string>> _listContainer = [];

        public string? GetFirefoxCookiePath { get; set; } = string.Empty;

        private readonly BrowserDeleteProcessInterfaceService bPIService = new();

        private readonly WindowsOS windowOS = new();

        public DeleteBrowserContentWindow(Dictionary<string, List<string>> fileContainer, string browserName, string browserIconPath, string additionalOption, bool startAutomaticDeletion)
        {
            InitializeComponent();

            this._listContainer = fileContainer;

            GetFirefoxCookiePath = additionalOption;

            TbtnDeleteBrowserContent.IsEnabled = false;

            tbl_choosen_browser.Text = browserName;

            this._browserName = browserName;


            BitmapImage browserIcon = new(new Uri(browserIconPath, UriKind.RelativeOrAbsolute));

            image_browser_icon.Source = browserIcon;

            _startAutomaticDeletion = startAutomaticDeletion;

            CbxDeleteCookies.IsChecked = true;
            CbxDeleteInternetCache.IsChecked = true;

            StartAutomaticDeleteProcess(startAutomaticDeletion);
        }


        private void StartAutomaticDeleteProcess(bool startAutomaticDeletion)
        {
            if(startAutomaticDeletion is true)
            {
                TbtnDeleteBrowserContent.IsChecked = true;
                DeleteProcess();
            }
        }

        private void TbtnDeleteBrowserContent_Click(object sender, RoutedEventArgs e)
        {
            if (TbtnDeleteBrowserContent.IsChecked is true)
            {
                if(bPIService.MatchChromeBrowser() is true)
                {
                    if (windowOS.ProcessAlive("chrome") is true)
                    {
                        BrowserIsStillOpen();
                        ResetDeleteTriggers();

                    } else
                    {
                        DeleteProcess();
                    }

                } else if(bPIService.MatchFirefoxBrowser() is true)
                {
                    if (windowOS.ProcessAlive("firefox") is true)
                    {
                        BrowserIsStillOpen();
                        ResetDeleteTriggers();
                    }
                    else
                    {
                        DeleteProcess();
                    }

                } else if(bPIService.MatchEdgeBrowser() is true)
                {
                    if (windowOS.ProcessAlive("msedge") is true)
                    {
                        BrowserIsStillOpen();
                        ResetDeleteTriggers();
                    }
                    else
                    {
                        DeleteProcess();
                    }

                }
            }
            else
            {
                _stopDeleteProcess = true;               
            }
        }

        private void BrowserIsStillOpen() {

            string message = $"Der {tbl_choosen_browser.Text} Browser muss geschlossen werden bevor Sie die ausgewählten Inhalte löschen können." + '\n' + '\n' + $"Möchten Sie den {tbl_choosen_browser.Text} jetzt schließen?";

            string title = "Browserinhalte löschen";

            MessageBoxResult result =  MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Error);

            if (result == MessageBoxResult.Yes) {

                switch (tbl_choosen_browser.Text)
                {
                    case "Firefox":

                        Debug.WriteLine("F");
                        windowOS.StopProcess("firefox");

                        break;

                    case "Chrome":

                        Debug.WriteLine("C");
                        windowOS.StopProcess("chrome");
                        break;

                    case "Edge":

                        Debug.WriteLine("Edge");

                        windowOS.StopProcess("msedge");

                        break;

                }           
            }

        }

        private void ResetDeleteTriggers()
        {
            _stopDeleteProcess = true;
            TbtnDeleteBrowserContent.IsChecked = false;
        }

        private void DeleteProcess()
        {
            _stopDeleteProcess = false;

            TbtnDeleteBrowserContent.Content = "Löschvorgang stoppen";
            Style? style = this.FindResource("StartTimerButtonMainMenuRed") as Style;
            TbtnDeleteBrowserContent.Style = style;

            if (CbxDeleteCookies.IsChecked is true && CbxDeleteInternetCache.IsChecked is true)
            {
                if (_listContainer.TryGetValue("InternetCacheAndCookies", out _))
                {
                    SetProgressbarValues(choosenPathsList = _listContainer["InternetCacheAndCookies"]);

                    Task.Run(() => StartBrowserContentDeleteProcessAsync(choosenPathsList));
                }
            }

            else if (CbxDeleteCookies.IsChecked is false && CbxDeleteInternetCache.IsChecked is true)
            {
                if (_listContainer.TryGetValue("InternetCache", out _))
                {
                    SetProgressbarValues(choosenPathsList = _listContainer["InternetCache"]);

                    Task.Run(() => StartBrowserContentDeleteProcessAsync(choosenPathsList));
                }
            }

            else if (CbxDeleteCookies.IsChecked is true && CbxDeleteInternetCache.IsChecked is false)
            {
                if (_listContainer.TryGetValue("InternetCookies", out _))
                {
                    SetProgressbarValues(choosenPathsList = _listContainer["InternetCookies"]);

                    Task.Run(() => StartBrowserContentDeleteProcessAsync(choosenPathsList));
                }
            }
        }

        private async Task StartBrowserContentDeleteProcessAsync(List<string> filePath)
        {
            await Task.Run(() => {

                foreach (string path in filePath)
                {
                    DeleteFilesAndSubdirectories(path);
                }

                this.Dispatcher.Invoke(new Action(async () => //Use Dispather to Update UI Immediately  
                {
                    if (_browserName!.Equals("Firefox") && (_listContainer.TryGetValue("InternetCacheAndCookies", out _) || _listContainer.TryGetValue("Cookies", out _)))
                    {
                        delete.DeleteSingleFile(GetFirefoxCookiePath!);
                        deleteProtocol.Text = $"Datei gelöscht: {GetFirefoxCookiePath}";
                    }

                    
                    InfoDeleteProcess();
                    ResetProgressBarSettings();
                    await Task.Delay(2000);

                    if(_startAutomaticDeletion is true) { this.Close(); }

                    deleteProtocol.Text = "-";
                    tbl_ProgressOfCleaningPercent.Text = "-";

                }));
            });
        }


        private void SetProgressbarValues(List<string> choosenPathsList)
        {
            Count count = new();
            _progressbarValueLimitAutomatic = count.CountNumberOfFilesInSpecifiedDirectories<int>(choosenPathsList);
            pbCleaningProgressbarAutomatic.Maximum = _progressbarValueLimitAutomatic;
        }

      
        private void ResetProgressBarSettings()
        {                             
                _progressbarValueLimitAutomatic         = 0;
                _progressbarValueIncrementAutomatic     = 0;
                pbCleaningProgressbarAutomatic.Value    = 0;
                tbl_ProgressOfCleaningPercent.Text      = "-";                
        }

        private void InfoDeleteProcess()
        {
            if(_stopDeleteProcess is true)
            {
                DeleteProcessResetToggleButton("Löschvorgang abgebrochen!");

            } else
            {
                DeleteProcessResetToggleButton("Löschvorgang erfolgreich!");
            }
         
        }

        private void DeleteProcessResetToggleButton(string a)
        {
            deleteProtocol.Text = a;
            TbtnDeleteBrowserContent.Content = "Ausgewählte Inhalte löschen";
            Style? style = this.FindResource("StartTimerButtonMainMenuGreen") as Style;
            TbtnDeleteBrowserContent.Style = style;
            TbtnDeleteBrowserContent.IsChecked = false;
            _stopDeleteProcess = true;
        }

        private void InfoDeleteProcessStopped()
        {
            this.Dispatcher.Invoke(new Action(async () =>
            {
                deleteProtocol.Text = "Löschvorgang abgebrochen!";
                await Task.Delay(2000);

            }));
        }

        private void DeleteFilesAndSubdirectories(string path)
        {
            // Delete files
            try
            {
                string[] files = Directory.GetFiles(path);

                foreach (string file in files)
                {
                    if (_stopDeleteProcess == true) {

                        //Debug.WriteLine("Stopped");

                        InfoDeleteProcessStopped(); break;
                    
                    }

                    if (file.Contains("moz-extension"))
                    {
                        //Do nothing
                    }
                    else
                    {
                        File.Delete(file);
                        
                        _progressbarValueIncrementAutomatic++;

                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            pbCleaningProgressbarAutomatic.Value = _progressbarValueIncrementAutomatic;
                            tbl_ProgressOfCleaningPercent.Text = ((_progressbarValueIncrementAutomatic * 100) / _progressbarValueLimitAutomatic) + " %";

                            deleteProtocol.Text = $"Datei gelöscht: {file}";

                        }));
                    }
                }

            } catch(Exception) { }


            try
            {
                string[] subdirectories = Directory.GetDirectories(path);
            
                foreach (string subdirectory in subdirectories)
                {
                    if (_stopDeleteProcess == true) { InfoDeleteProcessStopped(); break; }
        
                        DeleteFilesAndSubdirectories(subdirectory);

                        Directory.Delete(subdirectory, false);                           
                }
            }
            catch (Exception)
            {

            }
        }


        private void CbxDeleteCookies_Checked(object sender, RoutedEventArgs e)
        {
            if (CbxDeleteCookies.IsChecked == true && CbxDeleteInternetCache.IsChecked == false)
            {
                TbtnDeleteBrowserContent.IsEnabled = true;
            }
        }

        private void CbxDeleteCookies_Unchecked(object sender, RoutedEventArgs e)
        {
            if (CbxDeleteCookies.IsChecked == false && CbxDeleteInternetCache.IsChecked == false)
            {
                TbtnDeleteBrowserContent.IsEnabled = false;
            }
            else if (CbxDeleteCookies.IsChecked == false && CbxDeleteInternetCache.IsChecked == true)
            {
                TbtnDeleteBrowserContent.IsEnabled = true;
            }
        }

        private void CbxDeleteInternetCache_Checked(object sender, RoutedEventArgs e)
        {
            if (CbxDeleteCookies.IsChecked == false && CbxDeleteInternetCache.IsChecked == true)
            {
                TbtnDeleteBrowserContent.IsEnabled = true;
            }
        }

        private void CbxDeleteInternetCache_Unchecked(object sender, RoutedEventArgs e)
        {
            if (CbxDeleteCookies.IsChecked == false && CbxDeleteInternetCache.IsChecked == false)
            {
                TbtnDeleteBrowserContent.IsEnabled = false;
            }
            else if (CbxDeleteCookies.IsChecked == true && CbxDeleteInternetCache.IsChecked == false)
            {
                TbtnDeleteBrowserContent.IsEnabled = true;
            }
        }

    }
}
