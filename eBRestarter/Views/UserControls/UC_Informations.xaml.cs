using System.Windows;
using System.Windows.Controls;

namespace eBRestarter.Views.UserControls
{

    public partial class UC_Informations : UserControl
    {

        public static readonly DependencyProperty TitleProperty             = DependencyProperty.Register(nameof(HeaderTitle),     typeof(string), typeof(UC_Informations));
        public static readonly DependencyProperty Title_Foreground_Property = DependencyProperty.Register(nameof(Title_Foreground), typeof(object), typeof(Control), new FrameworkPropertyMetadata { BindsTwoWayByDefault = true, });

        public static readonly DependencyProperty HeaderImageProperty       = DependencyProperty.Register(nameof(HeaderImage),     typeof(object), typeof(UC_Informations));
        public static readonly DependencyProperty BackGroundColorProperty   = DependencyProperty.Register(nameof(BackGroundColor), typeof(object), typeof(UC_Informations));

        public static readonly DependencyProperty ImageSizeHeightProperty   = DependencyProperty.Register(nameof(ImageSizeHeight), typeof(string), typeof(UC_Informations));
        public static readonly DependencyProperty ImageSizeWidthProperty    = DependencyProperty.Register(nameof(ImageSizeWidth),  typeof(string), typeof(UC_Informations));
 
        public static readonly DependencyProperty InformationRow_1_Property = DependencyProperty.Register(nameof(InformationRow_1), typeof(string), typeof(Control), new FrameworkPropertyMetadata { BindsTwoWayByDefault = true, });
        public static readonly DependencyProperty InformationRow_2_Property = DependencyProperty.Register(nameof(InformationRow_2), typeof(string), typeof(Control), new FrameworkPropertyMetadata { BindsTwoWayByDefault = true, });
        public static readonly DependencyProperty InformationRow_3_Property = DependencyProperty.Register(nameof(InformationRow_3), typeof(string), typeof(Control), new FrameworkPropertyMetadata { BindsTwoWayByDefault = true, });

        public static readonly DependencyProperty InformationRow_Foreground_Property = DependencyProperty.Register(nameof(InformationRow_Foreground), typeof(object), typeof(Control), new FrameworkPropertyMetadata { BindsTwoWayByDefault = true, });


        public string HeaderTitle
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public object HeaderImage
        {
            get { return (object)GetValue(HeaderImageProperty); }
            set { SetValue(HeaderImageProperty, value); }
        }

        public string ImageSizeHeight
        {
            get { return (string)GetValue(ImageSizeHeightProperty); }
            set { SetValue(ImageSizeHeightProperty, value); }
        }

        public string ImageSizeWidth
        {
            get { return (string)GetValue(ImageSizeWidthProperty); }
            set { SetValue(ImageSizeWidthProperty, value); }
        }

        public object BackGroundColor
        {
            get { return (object)GetValue(BackGroundColorProperty); }
            set { SetValue(BackGroundColorProperty, value); }
        }

        public string InformationRow_1
        {
            get { return (string)GetValue(InformationRow_1_Property); }
            set { SetValue(InformationRow_1_Property, value); }
        }

        public string InformationRow_2
        {
            get { return (string)GetValue(InformationRow_2_Property); }
            set { SetValue(InformationRow_2_Property, value); }
        }

        public string InformationRow_3
        {
            get { return (string)GetValue(InformationRow_3_Property); }
            set { SetValue(InformationRow_3_Property, value); }
        }

        public object InformationRow_Foreground
        {
            get { return (object)GetValue(InformationRow_Foreground_Property); }
            set { SetValue(InformationRow_3_Property, value); }
        }

        public object Title_Foreground
        {
            get { return (object)GetValue(Title_Foreground_Property); }
            set { SetValue(Title_Foreground_Property, value); }
        }

        public UC_Informations()
        {
            InitializeComponent();
            //InfocenterView.Model = infocenterViewModel;
            uc_information_backgroundcolor.DataContext = this;
            uc_information_header_image.DataContext = this;
            tbl_cpu_name.DataContext = this;
            tbl_gpu_name.DataContext = this;
            tbl_installed_RAM.DataContext = this;
            uc_information_title.DataContext = this;

        }
    }
}
