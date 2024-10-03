using eBRestarter.Classes.EBFiles;
using Serilog;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace eBRestarter.Classes.InternetBrowser
{
    public sealed partial class Firefox : Browser
    {
        //Classes variables
        
        private readonly string fireFoxRoamingFilePath              = @Path.GetPathRoot(Environment.SystemDirectory) + "\\Users\\" + Environment.UserName + "\\AppData\\Roaming\\Mozilla\\Firefox\\Profiles\\";
        private readonly string fireFoxRoamingFilePathProfile       = @Path.GetPathRoot(Environment.SystemDirectory) + "\\Users\\" + Environment.UserName + "\\AppData\\Roaming\\Mozilla\\Firefox\\profiles.ini";
        
        private readonly string fireFoxLocalFilePath                = @Path.GetPathRoot(Environment.SystemDirectory) + "\\Users\\" + Environment.UserName + "\\AppData\\Local\\Mozilla\\Firefox\\Profiles\\";
        
        private readonly string standardWindowsFirefoxBrowserPath   = @Path.GetPathRoot(Environment.SystemDirectory) + "\\Program Files\\Mozilla Firefox\\firefox.exe";

        private const    string MATCH_STRING                        = ".*Default=Profiles/.*";
        private const    string REPLACE_STRING                      = "Default=Profiles/";

        private const    string COOKIES_SECTIONS                    = "\\storage\\default";

        private const    string COOKIES_SQLITE                      = "\\cookies.sqlite";

        private const    string INTERNETCACHE_2_SECTIONS            = "\\cache2\\entries";
        private const    string INTERNETCACHE_2_DOOMED_SECTIONS     = "\\cache2\\doomed";
        private const    string INTERNET_JUMP_LIST_CACHE_SECTIONS   = "\\jumpListCache";

        private const    string EXTENSIONS_SECTIONS                 = "\\extensions";

        private readonly string _firefoxVersionRegPath              = @"HKEY_LOCAL_MACHINE\SOFTWARE\Mozilla\Mozilla Firefox";
        private const    string FIREFOX_PROCESS                     = "firefox.exe";

        public  const    string EBESUCHER_ADD_ON_NAME_FOR_FIREFOX   = "\\{fef425dc-a60f-4484-954d-71ecf2544846}.xpi";

        private string FireFoxLocalPath                 { get => fireFoxLocalFilePath + ProfileFirefoxName; }
        private string FireFoxRoamingPath               { get => fireFoxRoamingFilePath + ProfileFirefoxName; }
        public override string InternetCacheDirectory   { get => FireFoxLocalPath + INTERNETCACHE_2_SECTIONS; }
        public override string CookiesPathDirectory     { get => FireFoxRoamingPath + COOKIES_SECTIONS; }

        [GeneratedRegex(@"[^0-9-.]")]
        private static partial Regex MyRegex();

        public override string ExtensionsDirectory
        {
            get
            {
                return FireFoxRoamingPath + EXTENSIONS_SECTIONS + EBESUCHER_ADD_ON_NAME_FOR_FIREFOX;
            }
        }      

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

                directoryListInternetCacheAndCookies!.Add(FireFoxLocalPath + INTERNETCACHE_2_SECTIONS);
                directoryListInternetCacheAndCookies!.Add(FireFoxLocalPath + INTERNETCACHE_2_DOOMED_SECTIONS);
                directoryListInternetCacheAndCookies!.Add(FireFoxLocalPath + INTERNET_JUMP_LIST_CACHE_SECTIONS);

                return directoryListInternetCacheAndCookies;
            }
        }


        public override List<string> InternetCookies
        {
            get
            {
                var directoryListInternetCacheAndCookies = new List<string>();

                directoryListInternetCacheAndCookies!.Add(CookiesPathDirectory);

                return directoryListInternetCacheAndCookies;
            }
        }

        public override string? CookieFilePath => FireFoxRoamingPath + COOKIES_SQLITE;

        public override string BrowserVersion
        {
            get
            {
                #pragma warning disable CA1416
                string ffVersionBrowserRegex = Windows!.GetCurrentBrowserVersion(_firefoxVersionRegPath, "CurrentVersion", "")!;

                if (ffVersionBrowserRegex != null)
                {
                    try
                    {
                        string ffVersionBrowserRegex2 = MyRegex().Replace(ffVersionBrowserRegex, ""); // Remove all letters except digits and dots

                        return ffVersionBrowserRegex2[..^2]; // Remove the 64 value from the firefox version
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Es ist eine Ausnahme im Firefoxbereich aufgetreten. Logging...");

                        return null!;
                    }
                }
                else
                {
                    return null!;
                }
            }
        }

        public override bool BrowserExist
        {
            get
            {
                if (File.Exists(standardWindowsFirefoxBrowserPath) == true && !eBRestarter.Classes.OperatingSystem.WindowsOS.IsDirectory(standardWindowsFirefoxBrowserPath))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public override bool ExtensionsExist
        {
            get {

                if (File.Exists(ExtensionsDirectory))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }          
        }

        public string ProfileFirefoxName
        {           
            get {                 
                    return Datainfo.GetInfoFromFile(fireFoxRoamingFilePathProfile, MATCH_STRING, REPLACE_STRING);
                }      
        }

        public Firefox(){}

        //Inherits the parameters from the constructors of the abstract class browser
        public Firefox(string websiteLink) : base(websiteLink){}

        //Inherits the parameters from the constructors of the abstract class browser
        public Firefox(string websiteLink, string addedLinkComponent) : base(websiteLink, addedLinkComponent){}

        public override void StartBrowser()
        {
            try
            {
                Windows!.Process = new Process();
                Windows.Process.StartInfo.FileName = FIREFOX_PROCESS;
                Windows.Process.StartInfo.UseShellExecute = true;
                Windows.Process.StartInfo.Arguments = WebsiteLink + AddedLinkComponent;
                Windows.Process.Start();
            }
            catch (Exception e)
            {
                Log.Error(e, "Es ist eine Ausnahme im Firefoxbereich aufgetreten. Logging...");
            }
        }

        public override void CloseBrowser()
        {
            Windows.CloseApplication("firefox");
        }

    }
}
