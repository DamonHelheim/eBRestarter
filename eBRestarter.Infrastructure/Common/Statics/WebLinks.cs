#pragma warning disable S1075 // URIs should not be hardcoded
namespace eBRestarter.Infrastructure.Common.Statics;

/// <summary>
/// Static collection of external web links, browser download URLs, addon store links, and search queries.
/// </summary>
public static class WebLinks
{
    /// <summary>
    /// Base URL for the eBesucher surfbar.
    /// </summary>
    public const string EVisitorSurflink = "https://www.ebesucher.de/surfbar/";

    /// <summary>
    /// Registration URL for eBesucher account creation.
    /// </summary>
    public const string RegistrationLink = "https://www.ebesucher.de/";

    /// <summary>
    /// Direct download link for Mozilla Firefox (64-bit German installer).
    /// </summary>
    public const string FirefoxDownloadLink = "https://download.mozilla.org/?product=firefox-latest-ssl&os=win64&lang=de";

    // Source Chrome standalone download reference: https://www.askvg.com/official-link-to-download-google-chrome-standalone-offline-installer/
    /// <summary>
    /// Direct download link for Google Chrome standalone offline installer (64-bit German).
    /// </summary>
    public const string ChromeDownloadLinkDE = "https://dl.google.com/tag/s/appguid%3D%7B8A69D345-D564-463C-AFF1-A69D9E530F96%7D%26iid%3D%7BBDA729A9-07E5-8BE1-A6AD-C479380DD938%7D%26lang%3de%26browser%3D3%26usagestats%3D0%26appname%3DGoogle%2520Chrome%26needsadmin%3Dprefers%26ap%3Dx64-stable-statsdef_1%26installdataindex%3Dempty/chrome/install/ChromeStandaloneSetup64.exe";

    /// <summary>
    /// Direct download link for Microsoft Edge installer (Stable German).
    /// </summary>
    public const string EdgeDownloadLinkDE = "https://go.microsoft.com/fwlink/?linkid=2108834&Channel=Stable&language=de&brand=M100";

    /// <summary>
    /// Direct download link for Brave Browser installer.
    /// </summary>
    public const string BraveDownloadLinkDE = "https://laptop-updates.brave.com/download/BRV010";

    /// <summary>
    /// Direct download link for Vivaldi installer (64-bit).
    /// </summary>
    public const string VivaldiDownloadLinkDE = "https://vivaldi.com/download/Vivaldi.x64.exe";

    /// <summary>
    /// Link to the eBesucher addon in the Mozilla Add-on store.
    /// </summary>
    public const string FirefoxEVisitorAddOnLink = "https://addons.mozilla.org/en-US/android/addon/surf-click/";

    /// <summary>
    /// Link to the eBesucher extension in the Chrome Web Store.
    /// </summary>
    public const string ChromeEVisitorAddOnLink = "https://chromewebstore.google.com/detail/ebesucher-addon/agchmcconfdfcenopioeilpgjngelefk";

    /// <summary>
    /// Link to the eBesucher extension search in the Microsoft Edge Addons store.
    /// </summary>
    public const string EdgeEVisitorAddOnLink = "https://microsoftedge.microsoft.com/addons/search/ebesucher";

    /// <summary>
    /// Fallback search URL for Google Chrome browser.
    /// </summary>
    public const string ChromeSearchUrl = "https://www.google.com/search?q=Google+Chrome";

    /// <summary>
    /// Fallback search URL for Mozilla Firefox browser.
    /// </summary>
    public const string FirefoxSearchUrl = "https://www.google.com/search?q=Firefox";

    /// <summary>
    /// Fallback search URL for Microsoft Edge browser.
    /// </summary>
    public const string EdgeSearchUrl = "https://www.google.com/search?q=Microsoft+Edge";

    /// <summary>
    /// Fallback search URL for Brave browser.
    /// </summary>
    public const string BraveSearchUrl = "https://www.google.com/search?q=Brave";

    /// <summary>
    /// URL for Google search engine.
    /// </summary>
    public const string GoogleLink = "https://www.google.com";
}
