using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace eBRestarter.Views.UserControls
{
    /// <summary>
    /// Interaktionslogik für UC_Icon_Credits.xaml
    /// </summary>
    public partial class UC_Icon_Credits : UserControl
    {
        public BitmapImage ImageSource
        {
            get { return (BitmapImage)Image_source.Source; }
            set { Image_source.Source = value; }
        }

        public string TB_Icon_Creator
        {
            get { return TB_Icon_text.Text; }
            set { TB_Icon_text.Text = value; }
        }

        public string? HyperlinkContent
        {
            get { return HL_Icon_Link.Inlines.FirstInline?.ToString(); }
            set
            {
                HL_Icon_Link.Inlines.Clear();
                HL_Icon_Link.Inlines.Add(value);
            }
        }

        public Uri HyperlinkUri
        {
            get { return HL_Icon_Link.NavigateUri; }
            set { HL_Icon_Link.NavigateUri = value; }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        public UC_Icon_Credits()
        {
            InitializeComponent();
        }
    }
}
