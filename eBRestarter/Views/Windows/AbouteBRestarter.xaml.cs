using eBRestarter.Services;
using eBRestarter.Views.UserControls;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using eBRestarter.Model.Models;

namespace eBRestarter.Views.Windows
{

    public partial class AbouteBRestarter : MetroWindow
    {

        private readonly List<IconCredit> _iconCreditList = [];

        public AbouteBRestarter()
        {
            InitializeComponent();

            tb_version_info.Text = $"Version: {VersionHelper.GetCurrentVersion!}";

            InitializeIconCreditList();

            foreach (var iconCredit in _iconCreditList)
            {

                var uc_icon_credit = new UC_Icon_Credits()
                {

                    TB_Icon_Creator = iconCredit.IconCreator,
                    ImageSource = new BitmapImage(new Uri(iconCredit.IconImageSource)),
                    HyperlinkContent = iconCredit.IconHyperLink,
                    HyperlinkUri = new Uri(iconCredit.IconHyperLinkContent)

                };

                SP_Icon_Container.Children.Add(uc_icon_credit);
            }         
        }


        private void InitializeIconCreditList()
        {
            _iconCreditList.Add(new IconCredit()
            {
                IconCreator = "Icon made by Hilmy Abiyyu A.from www.flaticon.com",
                IconImageSource = "pack://application:,,,/Resources/Images/Icons/Intersection/play.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/data-cleaning_2088794",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/data-cleaning_2088794"

            });

            _iconCreditList.Add(new IconCredit()
            {
                IconCreator = "Icon made by Freepik from www.flaticon.com",
                IconImageSource = "pack://application:,,,/Resources/Images/Icons/Intersection/timer.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/time_7821962",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/time_7821962"

            });

            _iconCreditList.Add(new IconCredit()
            {
                IconCreator = "Icon made by Those Icons from www.flaticon.com",
                IconImageSource = "pack://application:,,,/Resources/Images/Icons/LightTheme/data-cleaning_light_theme.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/data-cleaning_2088794",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/data-cleaning_2088794"

            });

            _iconCreditList.Add(new IconCredit()
            {
                IconCreator = "Icon made by Karacis from www.flaticon.com",
                IconImageSource = "pack://application:,,,/Resources/Images/Icons/LightTheme/status_light_theme.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/status_4785876",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/status_4785876"

            });

            _iconCreditList.Add(new IconCredit()
            {
                IconCreator = "Icon made by Ilham Fitrotul Hayat from www.flaticon.com",
                IconImageSource = "pack://application:,,,/Resources/Images/Icons/Intersection/deactivate.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/cross_2763138",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/cross_2763138"

            });

            _iconCreditList.Add(new IconCredit()
            {
                IconCreator = "Icon made by creative_designer from www.flaticon.com",
                IconImageSource = "pack://application:,,,/Resources/Images/Icons/Intersection/stop.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/stop_7826834",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/stop_7826834"

            });

            _iconCreditList.Add(new IconCredit()
            {
                IconCreator = "Icon made by afif fudin from www.flaticon.com",
                IconImageSource = "pack://application:,,,/Resources/Images/Icons/Intersection/tasks.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/tasks_9764456",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/tasks_9764456"

            });

            _iconCreditList.Add(new IconCredit()
            {
                IconCreator = "Icon made by Maxim Basinski Premium from www.flaticon.com",
                IconImageSource = "pack://application:,,,/Resources/Images/Icons/LightTheme/clock_light_theme.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/clock_9229215",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/clock_9229215"

            });

            _iconCreditList.Add(new IconCredit()
            {
                IconCreator = "Icon made by kerismaker Blue from www.flaticon.com",
                IconImageSource = "pack://application:,,,/Resources/Images/Icons/LightTheme/ip-address_light_theme.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/ip-address_8686134",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/ip-address_8686134"

            });

            _iconCreditList.Add(new IconCredit()
            {
                IconCreator = "Icon made by See Icons from www.flaticon.com",
                IconImageSource = "pack://application:,,,/Resources/Images/Icons/Intersection/key.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/key_11229372",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/key_11229372"

            });

            _iconCreditList.Add(new IconCredit()
            {
                IconCreator = "Icon made by Freepik from www.flaticon.com",
                IconImageSource = "pack://application:,,,/Resources/Images/Icons/Intersection/file_information.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/test_3672470",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/test_3672470"

            });

            _iconCreditList.Add(new IconCredit()
            {
                IconCreator = "Icon made by Freepik from www.flaticon.com",
                IconImageSource = "pack://application:,,,/Resources/Images/Icons/Intersection/globe-grid.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/globe-grid_3719350",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/globe-grid_3719350"

            });

            _iconCreditList.Add(new IconCredit()
            {
                IconCreator = "Icon made by Royyan Wijaya from www.flaticon.com",
                IconImageSource = "pack://application:,,,/Resources/Images/Icons/LightTheme/download_light_theme.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/download_3018413",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/download_3018413"

            });

            _iconCreditList.Add(new IconCredit()
            {
                IconCreator = "Icon made by Freepik from www.flaticon.com",
                IconImageSource = "pack://application:,,,/Resources/Images/Icons/LightTheme/network-interface-card_light_theme.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/network-interface-card_1176920",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/network-interface-card_1176920"

            });

            _iconCreditList.Add(new IconCredit()
            {
                IconCreator = "Icon made by rukanicon from www.flaticon.com",
                IconImageSource = "pack://application:,,,/Resources/Images/Icons/Intersection/file.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/file_8096501",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/file_8096501"

            });

            _iconCreditList.Add(new IconCredit()
            {
                IconCreator = "Icon made by Senapedia from www.flaticon.com",
                IconImageSource = "pack://application:,,,/Resources/Images/Icons/LightTheme/add_document_light_theme.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/document_6053089",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/document_6053089"

            });

            _iconCreditList.Add(new IconCredit()
            {
                IconCreator = "Icon made by Good Ware from www.flaticon.com",
                IconImageSource = "pack://application:,,,/Resources/Images/Icons/Intersection/wrong_document.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/document_685201",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/document_685201"

            });

            _iconCreditList.Add(new IconCredit()
            {
                IconCreator = "Icon made by Freepik from www.flaticon.com",
                IconImageSource = "pack://application:,,,/Resources/Images/Icons/LightTheme/user_2_light_theme.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/profile-user_64572",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/profile-user_64572"

            });

            _iconCreditList.Add(new IconCredit()
            {
                IconCreator = "Icon made by Freepik from www.flaticon.com",
                IconImageSource = "pack://application:,,,/Resources/Images/Icons/LightTheme/user_light_theme.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/user_456212",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/user_456212"

            });

            _iconCreditList.Add(new IconCredit()
            {
                IconCreator = "Icon made by Uniconlabs from www.flaticon.com",
                IconImageSource = "pack://application:,,,/Resources/Images/Icons/LightTheme/circle_with_globe_light_theme.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/web_3178162",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/web_3178162"

            });

            _iconCreditList.Add(new IconCredit()
            {
                IconCreator = "Icon made by manshagraphics from www.flaticon.com",
                IconImageSource = "pack://application:,,,/Resources/Images/Icons/Intersection/api.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/api_9002406",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/api_9002406"

            });

            _iconCreditList.Add(new IconCredit()
            {
                IconCreator = "Icon made by Freepik from www.flaticon.com",
                IconImageSource = "pack://application:,,,/Resources/Images/Icons/Intersection/approval.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/approval_1292921",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/approval_1292921"

            });

            _iconCreditList.Add(new IconCredit()
            {
                IconCreator = "Icon made by Circlon Tech from www.flaticon.com",
                IconImageSource = "pack://application:,,,/Resources/Images/Icons/LightTheme/workstation_light_theme.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/workstation_8039540",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/workstation_8039540"

            });

            _iconCreditList.Add(new IconCredit()
            {
                IconCreator = "Icon made by Ranksol Graphics from www.flaticon.com",
                IconImageSource = "pack://application:,,,/Resources/Images/Icons/LightTheme/calendar_light_theme.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/calendar_9371643",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/calendar_9371643"

            });

            _iconCreditList.Add(new IconCredit()
            {
                IconCreator = "Icon made by Lizel Arina from www.flaticon.com",
                IconImageSource = "pack://application:,,,/Resources/Images/Icons/LightTheme/note_light_theme.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/note_7710761",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/note_7710761"

            });

            _iconCreditList.Add(new IconCredit()
            {
                IconCreator = "Icon made by Heisenberg_jr from www.flaticon.com",
                IconImageSource = "pack://application:,,,/Resources/Images/Icons/LightTheme/send-data-light_theme.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/send-data_8053605",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/send-data_8053605"

            });

            _iconCreditList.Add(new IconCredit()
            {
                IconCreator = "Icon made by Fuzzee from www.flaticon.com",
                IconImageSource = "pack://application:,,,/Resources/Images/Icons/Intersection/plus.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/add_2724647",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/add_2724647"

            });

            _iconCreditList.Add(new IconCredit()
            {
                IconCreator = "Icon made by Syahrul Ramadhany from www.flaticon.com",
                IconImageSource = "pack://application:,,,/Resources/Images/Icons/LightTheme/windows_light_theme.png",
                IconHyperLink = "https://www.flaticon.com/free-icon/window_3494371",
                IconHyperLinkContent = "https://www.flaticon.com/free-icon/window_3494371"

            });

        }
    }
}
