using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Domain.Models.Records;
using System.Diagnostics;
using System.Reflection;

namespace eBRestarter.Infrastructure.Services;

public class AppInfoService : IAppInfoService
{
    public string GetAppVersion()
    {
        // WinUI 3 Apps nutzen oft Package.Current.Id.Version,
        // aber für Desktop-Apps funktioniert Reflection weiterhin gut:
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
        return fvi.FileVersion ?? "1.0.0";
    }

    public List<IconCredit> GetIconCredits()
    {
        return
        [
            new IconCredit
            {
                IconCreator = "Icon made by Hilmy Abiyyu A. from www.flaticon.com",
                IconImageSource = "/Resources/Visuals/Icons/Intersection/play.png", // Hinweis: WinUI nutzt ms-appx:/// Pfade
                IconHyperLink = "https://www.flaticon.com/free-icon/data-cleaning_2088794",
                IconHyperLinkContent = "flaticon.com/data-cleaning"
            },
            new IconCredit()
            {
                IconCreator = "Icon made by Freepik from www.flaticon.com",
                IconImageSource = "/Resources/Visuals/Icons/Intersection/timer.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/time_7821962",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/time_7821962"
            },
            new IconCredit()
            {
                IconCreator = "Icon made by Those Icons from www.flaticon.com",
                IconImageSource = "/Resources/Visuals/Icons/LightTheme/data-cleaning_light_theme.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/data-cleaning_2088794",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/data-cleaning_2088794"
            },
            new IconCredit()
            {
                IconCreator = "Icon made by Karacis from www.flaticon.com",
                IconImageSource = "/Resources/Visuals/Icons/LightTheme/status_light_theme.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/status_4785876",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/status_4785876"
            },
            new IconCredit()
            {
                IconCreator = "Icon made by Ilham Fitrotul Hayat from www.flaticon.com",
                IconImageSource = "/Resources/Visuals/Icons/Intersection/deactivate.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/cross_2763138",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/cross_2763138"
            },
            new IconCredit()
            {
                IconCreator = "Icon made by creative_designer from www.flaticon.com",
                IconImageSource = "/Resources/Visuals/Icons/Intersection/stop.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/stop_7826834",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/stop_7826834"
            },
            new IconCredit()
            {
                IconCreator = "Icon made by afif fudin from www.flaticon.com",
                IconImageSource = "/Resources/Visuals/Icons/Intersection/tasks.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/tasks_9764456",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/tasks_9764456"
            },
            new IconCredit()
            {
                IconCreator = "Icon made by Shuvo.Das from www.flaticon.com",
                IconImageSource = "/Resources/Visuals/Icons/LightTheme/clock_light_theme.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/watch_13927704",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/watch_13927704"
            },
            new IconCredit()
            {
                IconCreator = "Icon made by kerismaker Blue from www.flaticon.com",
                IconImageSource = "/Resources/Visuals/Icons/LightTheme/ip-address_light_theme.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/ip-address_8686134",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/ip-address_8686134"
            },
            new IconCredit()
            {
                IconCreator = "Icon made by Pixel perfect Icons from www.flaticon.com",
                IconImageSource = "/Resources/Visuals/Icons/Intersection/fa_keyaccess.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/target_2891501",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/target_2891501"
            },
            new IconCredit()
            {
                IconCreator = "Icon made by heisenberg_jr from www.flaticon.com",
                IconImageSource = "/Resources/Visuals/Icons/Intersection/globe-grid.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/internet_10438779",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/internet_10438779"
            },
            new IconCredit()
            {
                IconCreator = "Icon made by Bharat Icons from www.flaticon.com",
                IconImageSource = "/Resources/Visuals/Icons/LightTheme/download_light_theme.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/downloads_7268609",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/downloads_7268609"
            },
            new IconCredit()
            {
                IconCreator = "Icon made by Hexagon075 from www.flaticon.com",
                IconImageSource = "/Resources/Visuals/Icons/LightTheme/network-interface-card_light_theme.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/line-card-leed_16319453",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/line-card-leed_16319453"
            },
            new IconCredit()
            {
                IconCreator = "Icon made by Saepul Nahwan from www.flaticon.com",
                IconImageSource = "/Resources/Visuals/Icons/Intersection/globe.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/globe_14627027",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/globe_14627027"
            },
            new IconCredit()
            {
                IconCreator = "Icon made by Freepik from www.flaticon.com",
                IconImageSource = "/Resources/Visuals/Icons/LightTheme/user_2_light_theme.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/profile-user_64572",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/profile-user_64572"
            },
            new IconCredit()
            {
                IconCreator = "Icon made by Freepik from www.flaticon.com",
                IconImageSource = "/Resources/Visuals/Icons/LightTheme/user_light_theme.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/user_456212",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/user_456212"
            },
            new IconCredit()
            {
                IconCreator = "Icon made by manshagraphics from www.flaticon.com",
                IconImageSource = "/Resources/Visuals/Icons/Intersection/api.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/api_9002406",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/api_9002406"
            },
            new IconCredit()
            {
                IconCreator = "Icon made by Circlon Tech from www.flaticon.com",
                IconImageSource = "/Resources/Visuals/Icons/LightTheme/workstation_light_theme.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/workstation_8039540",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/workstation_8039540"
            },
            new IconCredit()
            {
                IconCreator = "Icon made by apien from www.flaticon.com",
                IconImageSource = "/Resources/Visuals/Icons/LightTheme/calendar_light_theme.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/calendar_18349418",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/calendar_18349418"
            },
            new IconCredit()
            {
                IconCreator = "Icon made by Lizel Arina from www.flaticon.com",
                IconImageSource = "/Resources/Visuals/Icons/LightTheme/note_light_theme.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/note_7710761",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/note_7710761"
            },
            new IconCredit()
            {
                IconCreator = "Icon made by Heisenberg_jr from www.flaticon.com",
                IconImageSource = "/Resources/Visuals/Icons/LightTheme/send-data-light_theme.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/send-data_8053605",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/send-data_8053605"
            },
            new IconCredit()
            {
                IconCreator = "Icon made by Fuzzee from www.flaticon.com",
                IconImageSource = "/Resources/Visuals/Icons/Intersection/plus.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/add_2724647",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/add_2724647"
            },
            new IconCredit()
            {
                IconCreator = "Icon made by Syahrul Ramadhany from www.flaticon.com",
                IconImageSource = "/Resources/Visuals/Icons/LightTheme/windows_light_theme.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/window_3494371",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/window_3494371"
            },
            new IconCredit()
            {
                IconCreator = "Icon made by Icons8 from www.icons8.com",
                IconImageSource = "/Resources/Visuals/Icons/Intersection/icons8_vivaldi.png",
                IconHyperLink = "https://icons8.com/icon/qopg2DkQyGsl/vivaldi-web-browser",
                IconHyperLinkContent = "https://icons8.com/icon/qopg2DkQyGsl/vivaldi-web-browser"
            }
        ];
    }

}
