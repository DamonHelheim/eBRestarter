using System.Collections.Generic;
using eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.PresentationDTO;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Providers.Interfaces;

namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Providers;

public sealed class IconCreditProvider : IIconCreditProvider
{
    private const string IntersectionPath = "/Resources/Visuals/Icons/Intersection/";
    private const string LightThemePath = "/Resources/Visuals/Icons/LightTheme/";
    private const string FlaticonUrl = "https://www.flaticon.com/free-icon/";
    private const string Icons8Url = "https://icons8.com/icon/";

    public List<IconCredit> GetIconCredits()
    {
        static IconCredit CreateCredit(string creator, string source, string url) => new()
        {
            IconCreator = creator,
            IconImageSource = source,
            IconHyperLink = url,
            IconHyperLinkContent = url
        };

        return
        [
            CreateCredit("Icon made by Hilmy Abiyyu A. from www.flaticon.com", IntersectionPath + "play.png", FlaticonUrl + "data-cleaning_2088794"),
            CreateCredit("Icon made by Freepik from www.flaticon.com", IntersectionPath + "timer.png", FlaticonUrl + "time_7821962"),
            CreateCredit("Icon made by Those Icons from www.flaticon.com", LightThemePath + "data-cleaning_light_theme.png", FlaticonUrl + "data-cleaning_2088794"),
            CreateCredit("Icon made by Karacis from www.flaticon.com", LightThemePath + "status_light_theme.png", FlaticonUrl + "status_4785876"),
            CreateCredit("Icon made by Ilham Fitrotul Hayat from www.flaticon.com", IntersectionPath + "deactivate.png", FlaticonUrl + "cross_2763138"),
            CreateCredit("Icon made by creative_designer from www.flaticon.com", IntersectionPath + "stop.png", FlaticonUrl + "stop_7826834"),
            CreateCredit("Icon made by afif fudin from www.flaticon.com", IntersectionPath + "tasks.png", FlaticonUrl + "tasks_9764456"),
            CreateCredit("Icon made by Shuvo.Das from www.flaticon.com", LightThemePath + "clock_light_theme.png", FlaticonUrl + "watch_13927704"),
            CreateCredit("Icon made by kerismaker Blue from www.flaticon.com", LightThemePath + "ip-address_light_theme.png", FlaticonUrl + "ip-address_8686134"),
            CreateCredit("Icon made by Pixel perfect Icons from www.flaticon.com", IntersectionPath + "fa_keyaccess.png", FlaticonUrl + "target_2891501"),
            CreateCredit("Icon made by heisenberg_jr from www.flaticon.com", IntersectionPath + "globe-grid.png", FlaticonUrl + "internet_10438779"),
            CreateCredit("Icon made by Bharat Icons from www.flaticon.com", LightThemePath + "download_light_theme.png", FlaticonUrl + "downloads_7268609"),
            CreateCredit("Icon made by Hexagon075 from www.flaticon.com", LightThemePath + "network-interface-card_light_theme.png", FlaticonUrl + "line-card-leed_16319453"),
            CreateCredit("Icon made by Saepul Nahwan from www.flaticon.com", IntersectionPath + "globe.png", FlaticonUrl + "globe_14627027"),
            CreateCredit("Icon made by Freepik from www.flaticon.com", LightThemePath + "user_2_light_theme.png", FlaticonUrl + "profile-user_64572"),
            CreateCredit("Icon made by Freepik from www.flaticon.com", LightThemePath + "user_light_theme.png", FlaticonUrl + "user_456212"),
            CreateCredit("Icon made by manshagraphics from www.flaticon.com", IntersectionPath + "api.png", FlaticonUrl + "api_9002406"),
            CreateCredit("Icon made by Circlon Tech from www.flaticon.com", LightThemePath + "workstation_light_theme.png", FlaticonUrl + "workstation_8039540"),
            CreateCredit("Icon made by apien from www.flaticon.com", LightThemePath + "calendar_light_theme.png", FlaticonUrl + "calendar_18349418"),
            CreateCredit("Icon made by Heisenberg_jr from www.flaticon.com", LightThemePath + "send-data-light_theme.png", FlaticonUrl + "send-data_8053605"),
            CreateCredit("Icon made by Fuzzee from www.flaticon.com", IntersectionPath + "plus.png", FlaticonUrl + "add_2724647"),
            CreateCredit("Icon made by Syahrul Ramadhany from www.flaticon.com", LightThemePath + "windows_light_theme.png", FlaticonUrl + "window_3494371"),
            CreateCredit("Icon made by Icons8 from www.icons8.com", IntersectionPath + "icons8_vivaldi.png", Icons8Url + "qopg2DkQyGsl/vivaldi-web-browser")
        ];
    }
}

