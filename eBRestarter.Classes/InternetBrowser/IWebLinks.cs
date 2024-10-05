
namespace eBRestarter.Classes.InternetBrowser
{
    public interface IWebLinks
    {
        public const string EV_SURFLINK                     = "https://www.ebesucher.de/surfbar/";

        public const string REGISTRATION_LINK               = "https://www.ebesucher.de/";

        public const string FIREFOX_DOWNLOAD_LINK           = "https://download.mozilla.org/?product=firefox-latest-ssl&os=win64&lang=de";
        
        //Source Chromedownload: https://www.askvg.com/official-link-to-download-google-chrome-standalone-offline-installer/
        public const string CHROME_DOWNLOAD_LINK_DE         = "https://dl.google.com/tag/s/appguid%3D%7B8A69D345-D564-463C-AFF1-A69D9E530F96%7D%26iid%3D%7BBDA729A9-07E5-8BE1-A6AD-C479380DD938%7D%26lang%3de%26browser%3D3%26usagestats%3D0%26appname%3DGoogle%2520Chrome%26needsadmin%3Dprefers%26ap%3Dx64-stable-statsdef_1%26installdataindex%3Dempty/chrome/install/ChromeStandaloneSetup64.exe";
        
        //public const string EDGE_DOWNLOAD_LINK_DE         = "https://msedge.sf.dl.delivery.mp.microsoft.com/filestreamingservice/files/e761727c-958f-4cd6-8861-4e070ea7a6a8/MicrosoftEdgeEnterpriseX64.msi";
        public const string EDGE_DOWNLOAD_LINK_DE           = "https://go.microsoft.com/fwlink/?linkid=2108834&Channel=Stable&language=de&brand=M100"; //"https://msedge.sf.dl.delivery.mp.microsoft.com/filestreamingservice/files/50940088-a2ff-4d2c-acd0-80ae0ffe92fb/MicrosoftEdgeEnterpriseX64.msi";
        
        public const string FIREFOX_EBESUCHER_ADD_ON_LINK   = "https://addons.mozilla.org/de/firefox/addon/ebesucher-addon1/";
        public const string CHROME_EBESUCHER_ADD_ON_LINK    = "https://chromewebstore.google.com/detail/ebesucher-addon/agchmcconfdfcenopioeilpgjngelefk";
        public const string EDGE_EBESUCHER_ADD_ON_LINK      = "https://microsoftedge.microsoft.com/addons/search/ebesucher";

        public const string CHROME_SEARCH_REQUEST           = "https://www.google.com/search?q=Google+Chrome";
        public const string FIREFOX_SEARCH_REQUEST          = "https://www.google.com/search?q=Firefox";
        public const string EDGE_SEARCH_REQUEST             = "https://www.google.com/search?q=Micrsoft+Edge";

        public const string GOOGLE_LINK                     = "http://www.google.com";

        //https://go.microsoft.com/fwlink/?linkid=2108834&Channel=Stable&language=de&brand=M100
        //"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
    }
}
