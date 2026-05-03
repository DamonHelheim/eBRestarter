using System.Collections.Generic;
using eBRestarter.Desktop.WinUI3.Models;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;

namespace eBRestarter.Desktop.WinUI3.Services;

public class IconCreditService : IIconCreditService
{
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
            CreateCredit("Icon made by Hilmy Abiyyu A. from www.flaticon.com", "/Resources/Visuals/Icons/Intersection/play.png", "https://www.flaticon.com/free-icon/data-cleaning_2088794"),
            CreateCredit("Icon made by Freepik from www.flaticon.com", "/Resources/Visuals/Icons/Intersection/timer.png", "https://www.flaticon.com/free-icon/time_7821962"),
            CreateCredit("Icon made by Those Icons from www.flaticon.com", "/Resources/Visuals/Icons/LightTheme/data-cleaning_light_theme.png", "https://www.flaticon.com/free-icon/data-cleaning_2088794"),
            CreateCredit("Icon made by Karacis from www.flaticon.com", "/Resources/Visuals/Icons/LightTheme/status_light_theme.png", "https://www.flaticon.com/free-icon/status_4785876"),
            CreateCredit("Icon made by Ilham Fitrotul Hayat from www.flaticon.com", "/Resources/Visuals/Icons/Intersection/deactivate.png", "https://www.flaticon.com/free-icon/cross_2763138"),
            CreateCredit("Icon made by creative_designer from www.flaticon.com", "/Resources/Visuals/Icons/Intersection/stop.png", "https://www.flaticon.com/free-icon/stop_7826834"),
            CreateCredit("Icon made by afif fudin from www.flaticon.com", "/Resources/Visuals/Icons/Intersection/tasks.png", "https://www.flaticon.com/free-icon/tasks_9764456"),
            CreateCredit("Icon made by Shuvo.Das from www.flaticon.com", "/Resources/Visuals/Icons/LightTheme/clock_light_theme.png", "https://www.flaticon.com/free-icon/watch_13927704"),
            CreateCredit("Icon made by kerismaker Blue from www.flaticon.com", "/Resources/Visuals/Icons/LightTheme/ip-address_light_theme.png", "https://www.flaticon.com/free-icon/ip-address_8686134"),
            CreateCredit("Icon made by Pixel perfect Icons from www.flaticon.com", "/Resources/Visuals/Icons/Intersection/fa_keyaccess.png", "https://www.flaticon.com/free-icon/target_2891501"),
            CreateCredit("Icon made by heisenberg_jr from www.flaticon.com", "/Resources/Visuals/Icons/Intersection/globe-grid.png", "https://www.flaticon.com/free-icon/internet_10438779"),
            CreateCredit("Icon made by Bharat Icons from www.flaticon.com", "/Resources/Visuals/Icons/LightTheme/download_light_theme.png", "https://www.flaticon.com/free-icon/downloads_7268609"),
            CreateCredit("Icon made by Hexagon075 from www.flaticon.com", "/Resources/Visuals/Icons/LightTheme/network-interface-card_light_theme.png", "https://www.flaticon.com/free-icon/line-card-leed_16319453"),
            CreateCredit("Icon made by Saepul Nahwan from www.flaticon.com", "/Resources/Visuals/Icons/Intersection/globe.png", "https://www.flaticon.com/free-icon/globe_14627027"),
            CreateCredit("Icon made by Freepik from www.flaticon.com", "/Resources/Visuals/Icons/LightTheme/user_2_light_theme.png", "https://www.flaticon.com/free-icon/profile-user_64572"),
            CreateCredit("Icon made by Freepik from www.flaticon.com", "/Resources/Visuals/Icons/LightTheme/user_light_theme.png", "https://www.flaticon.com/free-icon/user_456212"),
            CreateCredit("Icon made by manshagraphics from www.flaticon.com", "/Resources/Visuals/Icons/Intersection/api.png", "https://www.flaticon.com/free-icon/api_9002406"),
            CreateCredit("Icon made by Circlon Tech from www.flaticon.com", "/Resources/Visuals/Icons/LightTheme/workstation_light_theme.png", "https://www.flaticon.com/free-icon/workstation_8039540"),
            CreateCredit("Icon made by apien from www.flaticon.com", "/Resources/Visuals/Icons/LightTheme/calendar_light_theme.png", "https://www.flaticon.com/free-icon/calendar_18349418"),
            CreateCredit("Icon made by Lizel Arina from www.flaticon.com", "/Resources/Visuals/Icons/LightTheme/note_light_theme.png", "https://www.flaticon.com/free-icon/note_7710761"),
            CreateCredit("Icon made by Heisenberg_jr from www.flaticon.com", "/Resources/Visuals/Icons/LightTheme/send-data-light_theme.png", "https://www.flaticon.com/free-icon/send-data_8053605"),
            CreateCredit("Icon made by Fuzzee from www.flaticon.com", "/Resources/Visuals/Icons/Intersection/plus.png", "https://www.flaticon.com/free-icon/add_2724647"),
            CreateCredit("Icon made by Syahrul Ramadhany from www.flaticon.com", "/Resources/Visuals/Icons/LightTheme/windows_light_theme.png", "https://www.flaticon.com/free-icon/window_3494371"),
            CreateCredit("Icon made by Icons8 from www.icons8.com", "/Resources/Visuals/Icons/Intersection/icons8_vivaldi.png", "https://icons8.com/icon/qopg2DkQyGsl/vivaldi-web-browser")
        ];
    }
}
