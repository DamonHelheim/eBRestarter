using eBRestarter.Classes.EBFiles;
using eBRestarter.Classes.EBInterfaces;
using eBRestarter.Classes.InternetBrowser;
using eBRestarter.Classes.OperatingSystem;
using eBRestarter.Classes.Services;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using eBRestarter.ViewModel.ViewModels;


namespace eBRestarter.Views.UserControls
{

    public partial class UC_Browser : UserControl
    {

        public static readonly DependencyProperty TitlePropertyBrowser = DependencyProperty.Register(nameof(HeaderTitleBrowser), typeof(string), typeof(UC_Browser));
        public static readonly DependencyProperty HeaderImagePropertyBrowser = DependencyProperty.Register(nameof(HeaderImageBrowser), typeof(string), typeof(UC_Browser));

        public static readonly DependencyProperty ImageSizeHeightPropertyBrowser = DependencyProperty.Register(nameof(ImageSizeHeightBrowser), typeof(string), typeof(UC_Browser));
        public static readonly DependencyProperty ImageSizeWidthPropertyBrowser = DependencyProperty.Register(nameof(ImageSizeWidthBrowser), typeof(string), typeof(UC_Browser));

        public static readonly DependencyProperty DownloadLinkProperty = DependencyProperty.Register(nameof(DownloadLink), typeof(string), typeof(UC_Browser));
        public static readonly DependencyProperty DownloadNameProperty = DependencyProperty.Register(nameof(DownloadName), typeof(string), typeof(UC_Browser));
        public static readonly DependencyProperty DownloadPathProperty = DependencyProperty.Register(nameof(DownloadPath), typeof(string), typeof(UC_Browser));
        public static readonly DependencyProperty DownloadToggleButtonVisibleProperty = DependencyProperty.Register(nameof(DownloadToggleButtonVisible), typeof(string), typeof(UC_Browser));

        public static readonly DependencyProperty BrowserVersionProperty = DependencyProperty.Register(nameof(BrowserVersion), typeof(string), typeof(Control), new FrameworkPropertyMetadata { BindsTwoWayByDefault = true, });
        public static readonly DependencyProperty BrowserExistProperty = DependencyProperty.Register(nameof(BrowserExist), typeof(string), typeof(Control), new FrameworkPropertyMetadata { BindsTwoWayByDefault = true, });

        public static readonly DependencyProperty BrowserExistTextForgroundProperty = DependencyProperty.Register(nameof(BrowserExistTextForground), typeof(string), typeof(Control), new FrameworkPropertyMetadata { BindsTwoWayByDefault = true, });

        public static readonly DependencyProperty ChooseBrowserButtonVisibleProperty = DependencyProperty.Register(nameof(ChooseBrowserButtonVisible), typeof(string), typeof(UC_Browser));


        public string HeaderTitleBrowser
        {
            get { return (string)GetValue(TitlePropertyBrowser); }
            set { SetValue(TitlePropertyBrowser, value); }
        }

        public string HeaderImageBrowser
        {
            get { return (string)GetValue(HeaderImagePropertyBrowser); }
            set { SetValue(HeaderImagePropertyBrowser, value); }
        }

        public string ImageSizeHeightBrowser
        {
            get { return (string)GetValue(ImageSizeHeightPropertyBrowser); }
            set { SetValue(ImageSizeHeightPropertyBrowser, value); }
        }

        public string ImageSizeWidthBrowser
        {
            get { return (string)GetValue(ImageSizeWidthPropertyBrowser); }
            set { SetValue(ImageSizeWidthPropertyBrowser, value); }
        }

        public string DownloadLink
        {
            get { return (string)GetValue(DownloadLinkProperty); }
            set { SetValue(DownloadLinkProperty, value); }
        }

        public string DownloadName
        {
            get { return (string)GetValue(DownloadNameProperty); }
            set { SetValue(DownloadNameProperty, value); }
        }

        public string DownloadPath
        {
            get { return (string)GetValue(DownloadPathProperty); }
            set { SetValue(DownloadPathProperty, value); }
        }

        public string DownloadToggleButtonVisible
        {
            get { return (string)GetValue(DownloadToggleButtonVisibleProperty); }
            set { SetValue(DownloadToggleButtonVisibleProperty, value); }
        }


        public string BrowserVersion
        {
            get { return (string)GetValue(BrowserVersionProperty); }
            set { SetValue(BrowserVersionProperty, value); }
        }


        public string BrowserExist
        {
            get { return (string)GetValue(BrowserExistProperty); }
            set { SetValue(BrowserExistProperty, value); }
        }

        public string BrowserExistTextForground
        {
            get { return (string)GetValue(BrowserExistTextForgroundProperty); }
            set { SetValue(BrowserExistTextForgroundProperty, value); }
        }

        public string ChooseBrowserButtonVisible
        {
            get { return (string)GetValue(ChooseBrowserButtonVisibleProperty); }
            set { SetValue(ChooseBrowserButtonVisibleProperty, value); }
        }

        //private HttpClient client;
        //private Progress<HttpProgressInfo> progress;

        private readonly IEBXMLFile eBXMLFileManagement = new EBXMLFileManagement();

        private Browser chromeBrowser = new Chrome();
        private Browser edgeBrowser = new Edge();
        private Browser firefoxBrowser = new Firefox();
        private Browser braveBrowser = new Brave();

        private readonly HttpClient client;
        private HttpResponseMessage? response;
        private long _totalBytes;
        private long _receivedBytes;

        public UC_Browser()
        {
            InitializeComponent();

            tbl_Browser_name.DataContext = this;
            uc_browser_image.DataContext = this;

            tbl_BrowserExist.DataContext = this;
            tbl_BrowserVersion.DataContext = this;

            BtnDownloadBrowser.DataContext = this;
            BtnChooseBrowser.DataContext = this;

            progressBarDownload.Visibility = Visibility.Hidden;

            client = new HttpClient();
        }

        private async void BtnDownloadBrowser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (BtnDownloadBrowser.IsChecked is true)
                {
                    progressBarDownload.Visibility = Visibility.Visible;
                    tbl_BrowserVersion.Visibility = Visibility.Hidden;

                    ToggleButton("Download" + '\n' + "abbrechen", "DownloadBrowserToggleButtonRed");

                    response = await client.GetAsync(DownloadLink, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    _totalBytes = response.Content.Headers.ContentLength ?? -1;
                    _receivedBytes = 0;

                    string filePath = Path.Combine(ISystemPaths.DownloadFolderPath, DownloadName);

                    using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                    {
                        using Stream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                        byte[] buffer = new byte[8192];
                        int bytesRead;

                        while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
                        {
                            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));

                            _receivedBytes += bytesRead;

                            UpdateProgress();

                            if (BtnDownloadBrowser.IsChecked is false)
                            {

                                client.CancelPendingRequests();

                                ToggleButton("Download", "DownloadBrowserToggleButton");
                                ResetProgressBar();

                                return;
                            }
                        }
                    }

                    if (BtnDownloadBrowser.IsChecked is true)
                    {
                        DownloadInstaller();
                        ToggleButton("Download", "DownloadBrowserToggleButton");
                        ResetProgressBar();
                    }
                }
            }
            catch (Exception e2)
            {
                Log.Error(e2, "Es ist eine Ausnahme im UC_Browserbereich aufgetreten. Logging...");
                ToggleButton("Download", "DownloadBrowserToggleButton");
                ResetProgressBar();
                DownloadFailedInfo();
            }
        }


        private async void DownloadFailedInfo()
        {
            InternetService internetService = new();

            bool isConnected = await internetService.IsInternetAvailable();

            if (isConnected is false)
            {

                string message = "Die Anwendung kann keine Internetverbindung erstellen.";

                string title = "Internetconnection";

                _ = MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

                return;

            }
            else {

                WindowsOS windows = new();

                string message = "Download Fehlgeschlagen! Wir leiten dich an die entsprechende Google Suche für den " + tbl_Browser_name.Text + " Download weiter." + '\n' + '\n' +
                                 "Möchtet du jetzt an die Googlesuche weiter geleitet werden?";

                string title = "Download " + tbl_Browser_name.Text;

                MessageBoxResult result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Error);

                if (result == MessageBoxResult.Yes)
                {
                    switch (tbl_Browser_name.Text)
                    {
                        case "Chrome":
                            windows.OpenLinkWithStandardbrowser(IWebLinks.CHROME_SEARCH_REQUEST);
                            break;

                        case "Firefox":
                            windows.OpenLinkWithStandardbrowser(IWebLinks.FIREFOX_SEARCH_REQUEST);
                            break;

                        case "Edge":
                            windows.OpenLinkWithStandardbrowser(IWebLinks.EDGE_SEARCH_REQUEST);
                            break;

                        case "Brave":
                            windows.OpenLinkWithStandardbrowser(IWebLinks.BAVE_SEARCH_REQUEST);
                            break;
                    }
                }
                else
                {
                }

            }
           
        }

        private void DownloadInstaller()
        {
            string message = "Download vollständig! Möchtest du die Installation des " + tbl_Browser_name.Text + " jetzt starten?";
            string title = "Installation " + tbl_Browser_name.Text;

            MessageBoxResult result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                WindowsOS windowsOS = new();

                string browserInstallerFilePath = ISystemPaths.DownloadFolderPath + DownloadName; //@"C:\Users\TestUser\Downloads\MicrosoftEdgeEnterpriseX64.msi";

                if (!File.Exists(browserInstallerFilePath))
                {
                    Debug.WriteLine("Die .msi-Datei wurde nicht gefunden: " + browserInstallerFilePath);
                    return;
                }

                windowsOS.StartExecute(browserInstallerFilePath);

                //if (tbl_Browser_name.Text.Equals("Edge"))
                //{

                //    windowsOS.StartMSIFile(browserInstallerFilePath);

                //} else
                //{

                //Process.Start(ISystemPaths.DownloadFolderPath + DownloadName);
                //}

            }
            else
            {
                string message2 = "Die Datei findest du jederzeit im Ordner \"Downloads\" wieder, falls du die Installation zu späteren Zeitpunkt starten möchtest.";
                string title2 = "Downloadort Download " + tbl_Browser_name.Text;

                MessageBox.Show(message2, title2, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void UpdateProgress()
        {
            if (_totalBytes > 0)
            {
                double progressPercentage = (double)_receivedBytes / _totalBytes * 100;
                progressBarDownload.Value = progressPercentage;

                tbl_DownloadSize.Text = string.Format("{0} MB's / {1} MB's", ((double)_receivedBytes / 1024d / 1024d).ToString("0.00"), (_totalBytes / 1024d / 1024d).ToString("0.00"));
            }
        }


        private void ToggleButton(string toggleButtonContent, string style)
        {
            BtnDownloadBrowser.Content = toggleButtonContent;
            Style? toggleButtonStyle = this.FindResource(style) as Style;
            BtnDownloadBrowser.Style = toggleButtonStyle;
        }

        private void ResetProgressBar()
        {
            progressBarDownload.Visibility = Visibility.Hidden;
            progressBarDownload.Value = 0;
            tbl_DownloadSize.Text = "";
            BtnDownloadBrowser.IsChecked = false;
            tbl_BrowserVersion.Visibility = Visibility.Visible;
        }


        private void BtnChooseBrowser_Click(object sender, RoutedEventArgs e)
        {
            if (HeaderTitleBrowser is "Chrome")
            {
                EBesucherAddOnMessage(HeaderTitleBrowser);
            }
            else if (HeaderTitleBrowser is "Edge")
            {
                EBesucherAddOnMessage(HeaderTitleBrowser);
            }
            else if (HeaderTitleBrowser is "Firefox")
            {
                EBesucherAddOnMessage(HeaderTitleBrowser);
            }
            else if (HeaderTitleBrowser is "Brave")
            {
                EBesucherAddOnMessage(HeaderTitleBrowser);
            }
        }


        private void SaveChoosenBrowser()
        {
            EBXMLFileService eBFileService = new(eBXMLFileManagement);

            eBFileService.WriteXMLAttributeInConfig(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.BrowserTag, IConfigConstants.BrowserTag_Selected_Attribut, tbl_Browser_name.Text as string);

            RestartBrowserTaskMediatorViewModel.ChossenBrowserMediator = eBFileService.GetXMLAttributeInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.BrowserTag, IConfigConstants.BrowserTag_Selected_Attribut);

            RestartBrowserTaskMediatorViewModel.TriggerChossenBrowserMediator = true;
        }

        private void EBesucherAddOnMessage(string headerTitleBrowser)
        {
            if (headerTitleBrowser is "Chrome" && chromeBrowser.ExtensionsExist is false)
            {
                MessageBoxResult MessageBoxResult = MessageBox.Show(MessageEVisitorAddOn(headerTitleBrowser), MessageEVisitorAddOnTitle(headerTitleBrowser), MessageBoxButton.YesNo, MessageBoxImage.Information);

                if (MessageBoxResult == MessageBoxResult.Yes)
                {
                    chromeBrowser = new Chrome(IWebLinks.CHROME_EBESUCHER_ADD_ON_LINK);

                    chromeBrowser.StartBrowser();
                }

                SaveChoosenBrowser();

            }
            else if (headerTitleBrowser is "Edge" && edgeBrowser.ExtensionsExist is false)
            {
                MessageBoxResult MessageBoxResult = MessageBox.Show(MessageEVisitorAddOn(headerTitleBrowser), MessageEVisitorAddOnTitle(headerTitleBrowser), MessageBoxButton.YesNo, MessageBoxImage.Information);

                if (MessageBoxResult == MessageBoxResult.Yes)
                {
                    edgeBrowser = new Edge(IWebLinks.EDGE_EBESUCHER_ADD_ON_LINK);

                    edgeBrowser.StartBrowser();
                }

                SaveChoosenBrowser();
            }
            else if (headerTitleBrowser is "Firefox" && firefoxBrowser.ExtensionsExist is false)
            {
                MessageBoxResult MessageBoxResult = MessageBox.Show(MessageEVisitorAddOn(headerTitleBrowser), MessageEVisitorAddOnTitle(headerTitleBrowser), MessageBoxButton.YesNo, MessageBoxImage.Information);

                if (MessageBoxResult == MessageBoxResult.Yes)
                {
                    firefoxBrowser = new Firefox(IWebLinks.FIREFOX_EBESUCHER_ADD_ON_LINK);

                    firefoxBrowser.StartBrowser();
                }

                SaveChoosenBrowser();

            }
            else if (headerTitleBrowser is "Brave" && braveBrowser.ExtensionsExist is false)
            {
                MessageBoxResult MessageBoxResult = MessageBox.Show(MessageEVisitorAddOn(headerTitleBrowser), MessageEVisitorAddOnTitle(headerTitleBrowser), MessageBoxButton.YesNo, MessageBoxImage.Information);

                if (MessageBoxResult == MessageBoxResult.Yes)
                {
                    braveBrowser = new Brave(IWebLinks.FIREFOX_EBESUCHER_ADD_ON_LINK);

                    braveBrowser.StartBrowser();
                }

                SaveChoosenBrowser();

            }
            else if (headerTitleBrowser is "Chrome" && chromeBrowser.ExtensionsExist is true)
            {

                SaveChoosenBrowser();

            }
            else if (headerTitleBrowser is "Edge" && edgeBrowser.ExtensionsExist is true)
            {

                SaveChoosenBrowser();

            }
            else if (headerTitleBrowser is "Firefox" && firefoxBrowser.ExtensionsExist is true)
            {

                SaveChoosenBrowser();
            }
            else if (headerTitleBrowser is "Brave" && braveBrowser.ExtensionsExist is true)
            {

                SaveChoosenBrowser();
            }
        }


        private string MessageEVisitorAddOnTitle(string browserTitle)
        {
            string title = "eBesucher Add-on für " + browserTitle + " ist nicht installiert";

            return title;
        }

        private string MessageEVisitorAddOn(string browserTitle)
        {
            string message = "Du musst zwingend das eBesucher Add-on installieren, damit du den " + browserTitle + "browser für das Surfen nutzen kannst." + "\r\n" + "\r\n" +
                             "Ohne die Installation des eBesucher Add-on startet die Surfbar nicht von automatisch." + "\r\n" + "\r\n" +

                             "Hinweis:" + "\r\n" + "\r\n" + 
                             "Sollte der eB Restarter nicht erkennen, dass das Add-On bereits im " + browserTitle + " Browser hinzugefügt worden ist, dann kannst du trotzdem den eB Restarter mit deinem Browser starten." + "\r\n" + "\r\n" +

                             "Wir würden dich jetzt direkt auf den Webstore von " + browserTitle + " für das eBesucher Add-on weiterleiten, damit du dieses von dort aus installieren kannst." + "\r\n" + "\r\n" +
                             "Möchtest du zum " + browserTitle + " - Webstore weitergeleitet werden?";

            return message;

        }
    }
}
