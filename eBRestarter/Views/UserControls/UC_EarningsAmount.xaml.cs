using System.Windows;
using System.Windows.Controls;

namespace eBRestarter.Views.UserControls
{

    public partial class UC_EarningsAmount : UserControl
    {
        public static readonly DependencyProperty TitlePropertyEarnings = DependencyProperty.Register(nameof(HeaderTitleEarnings), typeof(string), typeof(Control), new FrameworkPropertyMetadata { BindsTwoWayByDefault = true, });
        public static readonly DependencyProperty EarnigsAmountProperty = DependencyProperty.Register(nameof(EarningsAmount), typeof(string), typeof(Control), new FrameworkPropertyMetadata { BindsTwoWayByDefault = true, });
        public static readonly DependencyProperty HeaderImagePropertyEarnings = DependencyProperty.Register(nameof(HeaderImageEarnings), typeof(object), typeof(UC_EarningsAmount));


        public UC_EarningsAmount()
        {
            InitializeComponent();
            tbl_sum_points_daily.DataContext = this;
            headerImage.DataContext = this;
            tbl_EarningsDay.DataContext = this;
        }

        public string HeaderTitleEarnings
        {
            get { return (string)GetValue(TitlePropertyEarnings); }
            set { SetValue(TitlePropertyEarnings, value); }
        }

        public object HeaderImageEarnings
        {
            get { return GetValue(HeaderImagePropertyEarnings); }
            set { SetValue(HeaderImagePropertyEarnings, value); }
        }

        public string EarningsAmount
        {
            get { return (string)GetValue(EarnigsAmountProperty); }
            set { SetValue(EarnigsAmountProperty, value); }
        }

    }
}
