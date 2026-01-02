using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Domain.Models.Records;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace eBRestarter.Infrastructure.Services
{
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
                    IconCreator = "Icon made by Maxim Basinski Premium from www.flaticon.com",
                    IconImageSource = "/Resources/Visuals/Icons/LightTheme/clock_light_theme.png",
                    IconHyperLink = "https://www.flaticon.com/free-icon/clock_9229215",
                    IconHyperLinkContent = "https://www.flaticon.com/free-icon/clock_9229215"
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
                    IconCreator = "Icon made by See Icons from www.flaticon.com",
                    IconImageSource = "/Resources/Visuals/Icons/Intersection/key.png",
                    IconHyperLink = "https://www.flaticon.com/free-icon/key_11229372",
                    IconHyperLinkContent = "https://www.flaticon.com/free-icon/key_11229372"
                },
                new IconCredit()
                {
                    IconCreator = "Icon made by Freepik from www.flaticon.com",
                    IconImageSource = "/Resources/Visuals/Icons/Intersection/file_information.png",
                    IconHyperLink = "https://www.flaticon.com/free-icon/test_3672470",
                    IconHyperLinkContent = "https://www.flaticon.com/free-icon/test_3672470"
                },
                new IconCredit()
                {
                    IconCreator = "Icon made by Freepik from www.flaticon.com",
                    IconImageSource = "/Resources/Visuals/Icons/Intersection/globe-grid.png",
                    IconHyperLink = "https://www.flaticon.com/free-icon/globe-grid_3719350",
                    IconHyperLinkContent = "https://www.flaticon.com/free-icon/globe-grid_3719350"
                },
                new IconCredit()
                {
                    IconCreator = "Icon made by Royyan Wijaya from www.flaticon.com",
                    IconImageSource = "/Resources/Visuals/Icons/LightTheme/download_light_theme.png",
                    IconHyperLink = "https://www.flaticon.com/free-icon/download_3018413",
                    IconHyperLinkContent = "https://www.flaticon.com/free-icon/download_3018413"
                },
                new IconCredit()
                {
                    IconCreator = "Icon made by Freepik from www.flaticon.com",
                    IconImageSource = "/Resources/Visuals/Icons/LightTheme/network-interface-card_light_theme.png",
                    IconHyperLink = "https://www.flaticon.com/free-icon/network-interface-card_1176920",
                    IconHyperLinkContent = "https://www.flaticon.com/free-icon/network-interface-card_1176920"
                },
                new IconCredit()
                {
                    IconCreator = "Icon made by rukanicon from www.flaticon.com",
                    IconImageSource = "/Resources/Visuals/Icons/Intersection/file.png",
                    IconHyperLink = "https://www.flaticon.com/free-icon/file_8096501",
                    IconHyperLinkContent = "https://www.flaticon.com/free-icon/file_8096501"
                },
                new IconCredit()
                {
                    IconCreator = "Icon made by Senapedia from www.flaticon.com",
                    IconImageSource = "/Resources/Visuals/Icons/LightTheme/add_document_light_theme.png",
                    IconHyperLink = "https://www.flaticon.com/free-icon/document_6053089",
                    IconHyperLinkContent = "https://www.flaticon.com/free-icon/document_6053089"
                },
                new IconCredit()
                {
                    IconCreator = "Icon made by Good Ware from www.flaticon.com",
                    IconImageSource = "/Resources/Visuals/Icons/Intersection/wrong_document.png",
                    IconHyperLink = "https://www.flaticon.com/free-icon/document_685201",
                    IconHyperLinkContent = "https://www.flaticon.com/free-icon/document_685201"
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
                    IconCreator = "Icon made by Uniconlabs from www.flaticon.com",
                    IconImageSource = "/Resources/Visuals/Icons/LightTheme/circle_with_globe_light_theme.png",
                    IconHyperLink = "https://www.flaticon.com/free-icon/web_3178162",
                    IconHyperLinkContent = "https://www.flaticon.com/free-icon/web_3178162"
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
                    IconCreator = "Icon made by Freepik from www.flaticon.com",
                    IconImageSource = "/Resources/Visuals/Icons/Intersection/approval.png",
                    IconHyperLink = "https://www.flaticon.com/free-icon/approval_1292921",
                    IconHyperLinkContent = "https://www.flaticon.com/free-icon/approval_1292921"
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
                    IconCreator = "Icon made by Ranksol Graphics from www.flaticon.com",
                    IconImageSource = "/Resources/Visuals/Icons/LightTheme/calendar_light_theme.png",
                    IconHyperLink = "https://www.flaticon.com/free-icon/calendar_9371643",
                    IconHyperLinkContent = "https://www.flaticon.com/free-icon/calendar_9371643"
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
                }
            ];
        }

    }
}
