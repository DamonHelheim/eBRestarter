using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace eBRestarter.Classes.InternetBrowser
{
    public partial class Brave : Browser, IChromiumCommonalities
    {
        //Classes variables
        private readonly string _braveLocalFilePath = @Path.GetPathRoot(Environment.SystemDirectory) + "\\Users\\" + Environment.UserName + "\\AppData\\Local\\BraveSoftware\\Brave-Browser\\User Data";

        private readonly string _standardWindowsBraveBrowserPathx64 = @Path.GetPathRoot(Environment.SystemDirectory) + "\\Program Files\\BraveSoftware\\Brave-Browser\\Application\\brave.exe";

        private const string BRAVE_VERSION = @"HKEY_CURRENT_USER\SOFTWARE\BraveSoftware\Brave-Browser\BLBeacon";
        private const string BRAVE_PROCESS = "brave.exe";

        private const string EBESUCHER_ADD_ON_NAME_FOR_BRAVE = "\\agchmcconfdfcenopioeilpgjngelefk";

        private string DefaultDirectoryBrave { get => _braveLocalFilePath + IChromiumCommonalities.DEFAULTSECTIONS; }
        public override string InternetCacheDirectory { get => DefaultDirectoryBrave + IChromiumCommonalities.INTERNET_CACHE_SECTIONS; }
        public override string CookiesPathDirectory { get => DefaultDirectoryBrave + IChromiumCommonalities.COOKIE_SECTIONS; }
        public override string ExtensionsDirectory { get => DefaultDirectoryBrave + IChromiumCommonalities.EXTENSIONS + EBESUCHER_ADD_ON_NAME_FOR_BRAVE; }


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

                directoryListInternetCacheAndCookies!.Add(DefaultDirectoryBrave + IChromiumCommonalities.INTERNET_CACHE_SERVICE_WORKER);
                directoryListInternetCacheAndCookies!.Add(DefaultDirectoryBrave + IChromiumCommonalities.INTERNET_CACHE_SECTIONS);

                return directoryListInternetCacheAndCookies;
            }
        }

        public override List<string> InternetCookies
        {
            get
            {
                var directoryListInternetCacheAndCookies = new List<string>();

                directoryListInternetCacheAndCookies!.Add(DefaultDirectoryBrave + IChromiumCommonalities.COOKIE_SECTIONS);
                directoryListInternetCacheAndCookies!.Add(DefaultDirectoryBrave + IChromiumCommonalities.COOKIES);

                return directoryListInternetCacheAndCookies;
            }
        }

        public override bool BrowserExist
        {
            get
            {
                //if (File.Exists(_standardWindowsBraveBrowserPathx64) == true && OperatingSystem.WindowsOS.IsDirectory(_standardWindowsBraveBrowserPathx64))
                if (File.Exists(_standardWindowsBraveBrowserPathx64) == true)
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

        public Brave() { }

        //Inherits the parameters from the constructors of the abstract class browser
        public Brave(string websiteLink) : base(websiteLink) { }

        //Inherits the parameters from the constructors of the abstract class browser
        public Brave(string websiteLink, string addedLinkComponent) : base(websiteLink, addedLinkComponent) { }

        public override void CloseBrowser()
        {
            Windows!.CloseApplication("brave");
        }

        public override void StartBrowser()
        {
            try
            {
                #pragma warning disable CA1416
                Windows!.Process = new Process();
                Windows.Process.StartInfo.FileName = BRAVE_PROCESS;
                Windows.Process.StartInfo.UseShellExecute = true;
                Windows.Process.StartInfo.Arguments = WebsiteLink + AddedLinkComponent;
                Windows.Process.Start();
            }
            catch (Exception e)
            {

                Log.Error(e, "Es ist eine Ausnahme im Bravebereich aufgetreten. Logging...");

            }
        }
        [GeneratedRegex(@"[^0-9-.]")]
        private static partial Regex MyRegex();

        public override string BrowserVersion
        {
            get
            {

                string? CVersionBrowserRegex = Windows!.GetCurrentBrowserVersion(BRAVE_VERSION, "version", "");

                if (CVersionBrowserRegex != null)
                {
                    try
                    {
                        string CVersionBrowserRegex2 = MyRegex().Replace(CVersionBrowserRegex, ""); // Remove all letters except digits and dots

                        //return CVersionBrowserRegex2.Substring(0, CVersionBrowserRegex2.Length - 0);
                        return CVersionBrowserRegex2[..];
                    }
                    catch (ArgumentOutOfRangeException e)
                    {
                        Log.Error(e, "Es ist eine Ausnahme im Bravebereich aufgetreten. Logging...");

                        return null!;
                    }
                }
                else
                {
                    return null!;
                }
            }
        }
    }
}
