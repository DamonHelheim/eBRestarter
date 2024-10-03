using eBRestarter.Classes.EBFiles;
using eBRestarter.Classes.EBInterfaces;
using eBRestarter.Classes.InternetBrowser;
using System.Diagnostics;

namespace eBRestarter.Classes.Services
{
    public class BrowserDeleteProcessInterfaceService
    {
        public string BrowserName                              { get; set; } = string.Empty;
        public string BrowserIconPath                          { get; set; } = string.Empty;
        public string FirefoxCookiePath                        { get; set; } = string.Empty;

        private List<string> InternetCacheAndCookies           { get; set; } = [];
        private List<string> InternetCache                     { get; set; } = [];
        private List<string> InternetCookies                   { get; set; } = [];

        public Dictionary<string, List<string>> ListContainer   { get; set; } = [];

        private readonly IEBXMLFile eBXMLFileManagement = new EBXMLFileManagement();

        public BrowserDeleteProcessInterfaceService() {

            Browser chromeBrowser   = new Chrome();
            Browser firefoxrowser   = new Firefox();
            Browser edgeBrowser     = new Edge();

            if (MatchChromeBrowser() is true)
            {
                InternetCacheAndCookies = chromeBrowser.InternetCacheAndCookiesDirectoryList!;
                InternetCache           = chromeBrowser.InternetCacheDirectoryList!;
                InternetCookies         = chromeBrowser.InternetCookies!;

                BrowserName             = "Chrome";
                BrowserIconPath         = @"/Resources/Images/Icons/Intersection/fa_chrome.png";
            }
            else if (MatchEdgeBrowser() is true)
            {
                InternetCacheAndCookies = edgeBrowser.InternetCacheAndCookiesDirectoryList!;
                InternetCache           = edgeBrowser.InternetCacheDirectoryList!;
                InternetCookies         = edgeBrowser.InternetCookies!;

                BrowserName             = "Edge";
                BrowserIconPath         = @"/Resources/Images/Icons/Intersection/fa_edge.png";
            }
            else if (MatchFirefoxBrowser() is true)
            {
                InternetCacheAndCookies = firefoxrowser.InternetCacheAndCookiesDirectoryList!;
                InternetCache           = firefoxrowser.InternetCacheDirectoryList!;
                InternetCookies         = firefoxrowser.InternetCookies!;
                FirefoxCookiePath       = firefoxrowser.CookieFilePath!;

                foreach(string a in InternetCacheAndCookies)
                {
                    Debug.WriteLine(a);
                }

                BrowserName     = "Firefox";
                BrowserIconPath = @"/Resources/Images/Icons/Intersection/fa_firefox.png";
            }

            ListContainer = new()
            {
                [nameof(InternetCacheAndCookies)]   = InternetCacheAndCookies!,
                [nameof(InternetCache)]             = InternetCache!,
                [nameof(InternetCookies)]           = InternetCookies!
            }; 
        }

        public bool MatchChromeBrowser()
        {
            EBXMLFileService eBFileService = new(eBXMLFileManagement);

            if (eBFileService.GetXMLAttributeInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.BrowserTag, IConfigConstants.BrowserTag_Selected_Attribut).Equals("Chrome"))
            {
                return true;
            }
            else { return false; }
        }

        public bool MatchFirefoxBrowser()
        {
            EBXMLFileService eBFileService = new(eBXMLFileManagement);

            if (eBFileService.GetXMLAttributeInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.BrowserTag, IConfigConstants.BrowserTag_Selected_Attribut).Equals("Firefox"))
            {
                return true;
            }
            else { return false; }
        }

        public bool MatchEdgeBrowser()
        {
            EBXMLFileService eBFileService = new(eBXMLFileManagement);

            if (eBFileService.GetXMLAttributeInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.BrowserTag, IConfigConstants.BrowserTag_Selected_Attribut).Equals("Edge"))
            {
                return true;
            }
            else { return false; }
        }
    }
}
