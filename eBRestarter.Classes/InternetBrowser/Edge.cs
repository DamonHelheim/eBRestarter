using Serilog;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace eBRestarter.Classes.InternetBrowser
{
    public partial class Edge : Browser, IChromiumCommonalities
    {

        private readonly string _edgeLocalFilePath           = @Path.GetPathRoot(Environment.SystemDirectory) + "\\Users\\" + Environment.UserName + "\\AppData\\Local\\Microsoft\\Edge\\User Data";
        private readonly string _standardWindowsEdgePathx86  = @Path.GetPathRoot(Environment.SystemDirectory) + "\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe";

        public const string EBESUCHER_ADD_ON_NAME_FOR_EDGE  = "\\kjhejmaladginnedpoppohfnkionnghi";
        private const string EDGE_VERSION                   = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Edge\BLBeacon";
        private const string EDGE_PROCESS                   = "msedge.exe";
        
        private string DefaultDirectoryEdge             { get => _edgeLocalFilePath + IChromiumCommonalities.DEFAULTSECTIONS; }
        public override string InternetCacheDirectory   { get => DefaultDirectoryEdge + IChromiumCommonalities.INTERNET_CACHE_SECTIONS; } // "C:\\Users\\TestUser\\AppData\\Local\\Google\\Chrome\\User Data\\Default\\Service Worker";
        public override string CookiesPathDirectory     { get => DefaultDirectoryEdge + IChromiumCommonalities.COOKIE_SECTIONS; } // "C:\\Users\\TestUser\\AppData\\Local\\Google\\Chrome\\User Data\\Default\\IndexedDB";
        public override string ExtensionsDirectory      { get => DefaultDirectoryEdge + IChromiumCommonalities.EXTENSIONS + EBESUCHER_ADD_ON_NAME_FOR_EDGE; }

        [GeneratedRegex(@"[^0-9-.]")]
        private static partial Regex MyRegex();

        public override List<string> InternetCacheAndCookiesDirectoryList
        {
            get
            {
                var directoryListInternetCacheAndCookies = new List<string>();

                directoryListInternetCacheAndCookies!.AddRange(InternetCacheDirectoryList);
                directoryListInternetCacheAndCookies!.AddRange(InternetCookies);

                return directoryListInternetCacheAndCookies;
            }
        }

        public override List<string> InternetCacheDirectoryList
        {
            get
            {
                var directoryListInternetCacheAndCookies = new List<string>();

                directoryListInternetCacheAndCookies!.Add(DefaultDirectoryEdge + IChromiumCommonalities.INTERNET_CACHE_SERVICE_WORKER);
                directoryListInternetCacheAndCookies!.Add(DefaultDirectoryEdge + IChromiumCommonalities.INTERNET_CACHE_SECTIONS);

                return directoryListInternetCacheAndCookies;
            }
        }

        public override List<string> InternetCookies
        {
            get
            {
                var directoryListInternetCacheAndCookies = new List<string>();

                directoryListInternetCacheAndCookies!.Add(DefaultDirectoryEdge + IChromiumCommonalities.COOKIE_SECTIONS);
                directoryListInternetCacheAndCookies!.Add(DefaultDirectoryEdge + IChromiumCommonalities.COOKIES);

                return directoryListInternetCacheAndCookies;
            }
        }


        public Edge(){}

        public Edge(string websiteLink) : base(websiteLink){}

        public Edge(string websiteLink, string addedLinkComponent) : base(websiteLink, addedLinkComponent){}

        public override void StartBrowser()
        {
            try
            {
                #pragma warning disable CA1416
                Windows!.Process = new Process();
                Windows.Process.StartInfo.FileName = EDGE_PROCESS;
                Windows.Process.StartInfo.UseShellExecute = true;
                Windows.Process.StartInfo.Arguments = WebsiteLink + AddedLinkComponent;
                Windows.Process.Start();
            }
            catch (Exception e)
            {
                Log.Error(e, "Es ist eine Ausnahme im Edgebereich aufgetreten. Logging...");
            }
        }

        public override void CloseBrowser()
        {
            Windows!.CloseApplication("msedge");
        }

        public override bool BrowserExist
        {
            get {
              
                if (File.Exists(_standardWindowsEdgePathx86) == true && !eBRestarter.Classes.OperatingSystem.WindowsOS.IsDirectory(_standardWindowsEdgePathx86))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }       
        }

        public override string BrowserVersion
        {
            get {

                string? CVersionBrowserRegex = Windows!.GetCurrentBrowserVersion(EDGE_VERSION, "version", "");

                if (CVersionBrowserRegex != null)
                {
                    try
                    {
                        string CVersionBrowserRegex2 = MyRegex().Replace(CVersionBrowserRegex, ""); // Remove all letters except digits and dots

                        //return CVersionBrowserRegex2.Substring(0, CVersionBrowserRegex2.Length - 0);

                        return CVersionBrowserRegex2[..];
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Es ist eine Ausnahme im Edgebereich aufgetreten. Logging...");

                        return null!;
                    }
                }
                else
                {
                    return null!;
                }
            }          
        }

        public override bool ExtensionsExist
        {
            get
            {
                if (Directory.Exists(ExtensionsDirectory))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }         
        }

    }
}
