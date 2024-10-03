using eBRestarter.Classes.OperatingSystem;
using System.Runtime.Versioning;

namespace eBRestarter.Classes.InternetBrowser
{
    public abstract class Browser
    {
        //Classes variables
        [SupportedOSPlatform("windows")]
        protected WindowsOS? Windows { get; set; } = new();

        //Property fields
        public string? WebsiteLink { get; set; }
        public string? AddedLinkComponent { get; set; }

        protected Browser()
        {
            WebsiteLink = string.Empty;
            AddedLinkComponent = string.Empty;
        }

        protected Browser(string websiteLink)
        {
            WebsiteLink = websiteLink;
        }

        protected Browser(string websiteLink, string addedLinkComponent)
        {
            WebsiteLink = websiteLink;
            AddedLinkComponent = addedLinkComponent;
        }

        //This area of the class summarizes all the methods that the classes to be inherited later have in common
        public abstract void StartBrowser();
        public abstract void CloseBrowser();

        public abstract string BrowserVersion { get; }
        public abstract bool BrowserExist { get; }

        public abstract string? InternetCacheDirectory { get; }
        public abstract string? CookiesPathDirectory { get; }
        public abstract string? ExtensionsDirectory { get; }
        public abstract bool ExtensionsExist { get; }

        public virtual List<string>? InternetCacheAndCookiesDirectoryList { get; }
        public virtual List<string>? InternetCacheDirectoryList { get; }
        public virtual List<string>? InternetCookies { get; }
        public virtual string? CookieFilePath { get; }

    }
}
