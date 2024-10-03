using System.Windows;
using System.Windows.Controls;
using eBRestarter.Services;

namespace eBRestarter.Views.UserControls
{
    public partial class UC_DashboardView : UserControl
    {
        public UC_DashboardView()
        {
            InitializeComponent();           
        }

        private void BtnGoToeVisitorAPI_Click(object sender, RoutedEventArgs e)
        {      
           ViewManager.LinkToAPIAccess<UC_OptionsView>();
        }

    }
}
